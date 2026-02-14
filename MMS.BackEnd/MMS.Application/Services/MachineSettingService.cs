namespace MMS.Application.Services;

public class MachineSettingService(
    IMachineSettingRepository machineSettingRepository,
    AutoMapperResult mapper,
    IUserContextService userContextService,
    IUnitOfWork unitOfWork,
    ICacheService cache
) : IMachineSettingService
{
    private static string GetByIdCacheKey(Guid id) => $"machineSetting:{id}";
    private static string GetListCacheKey(PageParameters p, MachineSettingsStatus? status, List<Guid> allowedIds)
        => $"machineSetting:list:{p.Term}:{p.Skip}:{p.Top}:{status}:{string.Join(",", allowedIds)}";

    public async Task<ApiResponse<MachineSettingDto>> AddAsync(AddMachineSettingDto request)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync();
        try
        {
            var machineResult = await unitOfWork.MachineRepository.GetAsync(request.MachineId);
            if (machineResult.IsLeft)
            {
                return new ApiResponse<MachineSettingDto>(
                    StatusCodes.Status200OK,
                    $"Machine with ID {request.MachineId} does not exist."
                );
            }
            var machine = machineResult.IfRight();
            await CustomerAccessHelper.ValidateCustomerAccessAsync(userContextService, machine!.CustomerId);

            if (await machineSettingRepository.ExistsByMachineIdAsync(request.MachineId))
            {
                return new ApiResponse<MachineSettingDto>(
                    StatusCodes.Status409Conflict,
                    $"MachineSetting already exists for Machine ID {request.MachineId}."
                );
            }
            var machineSetting = mapper.Map<MachineSetting>(request);
            var addedSetting = await machineSettingRepository.AddAsync(machineSetting);
            await unitOfWork.SaveChangesAsync();

            await cache.RemoveByPrefixAsync("machineSetting:list");

            await transaction.CommitAsync();

            return new ApiResponse<MachineSettingDto>(
                StatusCodes.Status201Created,
                nameof(MachineSetting) + ResponseMessages.Added,
                mapper.GetResult<MachineSetting, MachineSettingDto>(addedSetting)
            );
        }
        catch (UnauthorizedAccessException ex)
        {
            await transaction.RollbackAsync();
            return new ApiResponse<MachineSettingDto>(
                StatusCodes.Status403Forbidden,
                ex.Message
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new ApiResponse<MachineSettingDto>(
                StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}"
            );
        }
    }

    public async Task<ApiResponse<string>> DeleteAsync(Guid settingId)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync();
        try
        {
            var result = await machineSettingRepository.GetAsync(settingId);

            return await result.MatchAsync(
                async setting =>
                {
                    var machineResult = await unitOfWork.MachineRepository.GetAsync(setting.MachineId);
                    if (machineResult.IsLeft)
                    {
                        return new ApiResponse<string>(
                            StatusCodes.Status200OK,
                            $"Machine with ID {setting.MachineId} not found."
                        );
                    }

                    var machine = machineResult.IfRight();
                    await CustomerAccessHelper.ValidateCustomerAccessAsync(userContextService, machine!.CustomerId);

                    await machineSettingRepository.DeleteAsync(settingId);
                    await unitOfWork.SaveChangesAsync();

                    await cache.RemoveAsync(GetByIdCacheKey(settingId));
                    await cache.RemoveByPrefixAsync("machineSetting:list");

                    await transaction.CommitAsync();

                    return new ApiResponse<string>(
                        StatusCodes.Status200OK,
                        nameof(MachineSetting) + ResponseMessages.Deleted
                    );
                },
                error =>
                {
                    var statusCode = error switch
                    {
                        EntityNotFound => StatusCodes.Status404NotFound,
                        EntitySoftDeleted => StatusCodes.Status410Gone,
                        _ => StatusCodes.Status400BadRequest
                    };

                    return Task.FromResult(new ApiResponse<string>(
                        statusCode,
                        $"MachineSetting with ID {settingId} not accessible: {error.GetType().Name}"
                    ));
                }
            );
        }
        catch (UnauthorizedAccessException ex)
        {
            await transaction.RollbackAsync();
            return new ApiResponse<string>(
                StatusCodes.Status403Forbidden,
                ex.Message
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new ApiResponse<string>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred while deleting the machine setting: {ex.Message}"
            );
        }
    }

    public async Task<ApiResponse<IEnumerable<MachineSettingDto>>> GetListAsync(
        PageParameters pageParameters,
        MachineSettingsStatus? status,
        Guid? MachineId)
    {
        try
        {
            var allowedMachineIds = await GetAllowedMachineIdsAsync();
            if (!allowedMachineIds.Any())
            {
                return new ApiResponse<IEnumerable<MachineSettingDto>>(
                    StatusCodes.Status200OK,
                    nameof(MachineSetting) + ResponseMessages.GetAll,
                    Enumerable.Empty<MachineSettingDto>()
                );
            }

            var termNormalized = pageParameters.Term?.Trim().ToLower() ?? string.Empty;
            Expression<Func<MachineSetting, bool>> searchExpr = _ => true;
            var settingFilters = new List<Expression<Func<MachineSetting, bool>>>();

            if (status.HasValue)
                settingFilters.Add(s => s.Status == status);

            settingFilters.Add(s => allowedMachineIds.Contains(s.MachineId));

            if (MachineId.HasValue && MachineId != Guid.Empty)
            {
                settingFilters.Add(s => s.MachineId == MachineId.Value);
            }

            var machineSettings = await machineSettingRepository.GetListAsync(
                pageParameters,
                searchExpr,
                settingFilters,
                q => q.OrderBy(s => s.CreatedAt)
            );

            var dtoList = mapper.Map<IEnumerable<MachineSettingDto>>(machineSettings);

            return new ApiResponse<IEnumerable<MachineSettingDto>>(
                StatusCodes.Status200OK,
                nameof(MachineSetting) + ResponseMessages.GetAll,
                dtoList
            );
        }
        catch (UnauthorizedAccessException ex)
        {
            return new ApiResponse<IEnumerable<MachineSettingDto>>(
                StatusCodes.Status403Forbidden,
                ex.Message
            );
        }
        catch (Exception ex)
        {
            return new ApiResponse<IEnumerable<MachineSettingDto>>(
                StatusCodes.Status500InternalServerError,
                "An error occurred while retrieving the machine settings: " + ex.Message
            );
        }
    }

    public async Task<ApiResponse<MachineSettingDto>> GetByIdAsync(Guid settingId)
    {
        try
        {
            var cacheKey = GetByIdCacheKey(settingId);
            var cached = await cache.GetAsync<MachineSettingDto>(cacheKey);
            if (cached is not null)
            {
                return new ApiResponse<MachineSettingDto>(
                    StatusCodes.Status200OK,
                    nameof(MachineSetting) + ResponseMessages.cache,
                    cached
                );
            }

            var result = await machineSettingRepository.GetAsync(settingId);

            return await result.MatchAsync(
                async right =>
                {
                    var machineResult = await unitOfWork.MachineRepository.GetAsync(right.MachineId);
                    if (machineResult.IsLeft)
                    {
                        return new ApiResponse<MachineSettingDto>(
                            StatusCodes.Status200OK,
                            $"Machine with ID {right.MachineId} not found."
                        );
                    }

                    var machine = machineResult.IfRight();
                    await CustomerAccessHelper.ValidateCustomerAccessAsync(userContextService, machine!.CustomerId);

                    var dto = mapper.Map<MachineSettingDto>(right);

                    await cache.SetAsync(cacheKey, dto, TimeSpan.FromHours(1));

                    return new ApiResponse<MachineSettingDto>(
                        StatusCodes.Status200OK,
                        nameof(MachineSetting) + ResponseMessages.Get,
                        dto
                    );
                },
                error =>
                {
                    var statusCode = error switch
                    {
                        EntityNotFound => StatusCodes.Status404NotFound,
                        EntitySoftDeleted => StatusCodes.Status410Gone,
                        _ => StatusCodes.Status400BadRequest
                    };

                    return Task.FromResult(new ApiResponse<MachineSettingDto>(
                        statusCode,
                        $"MachineSetting with ID {settingId} not accessible: {error.GetType().Name}"
                    ));
                }
            );
        }
        catch (UnauthorizedAccessException ex)
        {
            return new ApiResponse<MachineSettingDto>(
                StatusCodes.Status403Forbidden,
                ex.Message
            );
        }
        catch (Exception ex)
        {
            return new ApiResponse<MachineSettingDto>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred while retrieving the machine setting: {ex.Message}"
            );
        }
    }

    public async Task<ApiResponse<MachineSettingDto>> UpdateAsync(Guid settingId, UpdateMachineSettingDto request)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync();
        try
        {
            var result = await machineSettingRepository.GetAsync(settingId);
            return await result.MatchAsync(
                async setting =>
                {
                    var machineResult = await unitOfWork.MachineRepository.GetAsync(setting.MachineId);
                    if (machineResult.IsLeft)
                    {
                        return new ApiResponse<MachineSettingDto>(
                            StatusCodes.Status200OK,
                            $"Machine with ID {setting.MachineId} not found."
                        );
                    }

                    var machine = machineResult.IfRight();
                    await CustomerAccessHelper.ValidateCustomerAccessAsync(userContextService, machine!.CustomerId);

                    var newMachineResult = await unitOfWork.MachineRepository.GetAsync(request.MachineId);
                    if (newMachineResult.IsLeft)
                    {
                        return new ApiResponse<MachineSettingDto>(
                            StatusCodes.Status200OK,
                            $"Machine with ID {request.MachineId} not found."
                        );
                    }

                    var newMachine = newMachineResult.IfRight();
                    await CustomerAccessHelper.ValidateCustomerAccessAsync(userContextService, newMachine.CustomerId);

                    mapper.Map(request, setting);
                    var updatedSetting = await machineSettingRepository.UpdateAsync(settingId, setting);
                    await unitOfWork.SaveChangesAsync();

                    await cache.RemoveAsync(GetByIdCacheKey(settingId));
                    await cache.RemoveByPrefixAsync("machineSetting:list");

                    await transaction.CommitAsync();

                    return new ApiResponse<MachineSettingDto>(
                        StatusCodes.Status200OK,
                        nameof(MachineSetting) + ResponseMessages.Updated,
                        mapper.GetResult<MachineSetting, MachineSettingDto>(updatedSetting)
                    );
                },
                error =>
                {
                    var statusCode = error switch
                    {
                        EntityNotFound => StatusCodes.Status404NotFound,
                        EntitySoftDeleted => StatusCodes.Status410Gone,
                        _ => StatusCodes.Status400BadRequest
                    };

                    return Task.FromResult(new ApiResponse<MachineSettingDto>(
                        statusCode,
                        $"MachineSetting with ID {settingId} not found or not accessible: {error.GetType().Name}"
                    ));
                }
            );
        }
        catch (UnauthorizedAccessException ex)
        {
            await transaction.RollbackAsync();
            return new ApiResponse<MachineSettingDto>(
                StatusCodes.Status403Forbidden,
                ex.Message
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new ApiResponse<MachineSettingDto>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred: {ex.Message}"
            );
        }
    }

    private async Task<List<Guid>> GetAllowedMachineIdsAsync()
    {
        try
        {
            Expression<Func<Machine, bool>> machineSearchExpr = m => true;
            var machineFilters = new List<Expression<Func<Machine, bool>>>();

            var customerIds = CustomerAccessHelper.GetAccessibleCustomerIds(userContextService);
            if (customerIds != null && customerIds.Any())
            {
                machineFilters.Add(m => customerIds.Contains(m.CustomerId));
            }

            var machines = await unitOfWork.MachineRepository.GetListAsync(
                null,
                machineSearchExpr,
                machineFilters,
                q => q.OrderBy(m => m.Id)
            );

            return machines.Select(m => m.Id).ToList();
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to fetch allowed machine IDs: {ex.Message}", ex);
        }
    }
}
