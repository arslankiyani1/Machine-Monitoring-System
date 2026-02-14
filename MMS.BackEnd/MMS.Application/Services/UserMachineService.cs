namespace MMS.Application.Services;

public class UserMachineService(
    IUserMachineRepository userMachineRepository,
    AutoMapperResult mapper,
    IUnitOfWork unitOfWork) : IUserMachineService
{
    public async Task<ApiResponse<UserMachineDto>> AddAsync(AddUserMachineDto request)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync();
        try
        {
            if (!await unitOfWork.MachineRepository.ExistsAsyncs(request.MachineId))
            {
                return new ApiResponse<UserMachineDto>(
                    StatusCodes.Status200OK,
                    $"Machine with ID {request.MachineId} does not exist."
                );
            }

            var exists = await userMachineRepository.ExistsAsync(x =>
                x.UserId == request.UserId && x.MachineId == request.MachineId);

            if (exists)
            {
                return new ApiResponse<UserMachineDto>(
                    StatusCodes.Status409Conflict,
                    $"UserMachine mapping already exists for User ID {request.UserId} and Machine ID {request.MachineId}."
                );
            }

            var userMachine = mapper.Map<UserMachine>(request);
            var added = await userMachineRepository.AddAsync(userMachine);
            await unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();

            return new ApiResponse<UserMachineDto>(
                StatusCodes.Status201Created,
                nameof(UserMachine) + ResponseMessages.Added,
                mapper.GetResult<UserMachine, UserMachineDto>(added)
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new ApiResponse<UserMachineDto>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred: {ex.Message}"
            );
        }
    }

    public async Task<ApiResponse<string>> DeleteAsync(Guid id)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync();

        try
        {
            var result = await userMachineRepository.GetAsync(id);

            return await result.MatchAsync(
                async userMachine =>
                {
                    await userMachineRepository.DeleteAsync(id);
                    await unitOfWork.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return new ApiResponse<string>(
                        StatusCodes.Status200OK,
                        nameof(UserMachine) + ResponseMessages.Deleted
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
                        $"UserMachine with ID {id} not accessible: {error.GetType().Name}"
                    ));
                }
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new ApiResponse<string>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred while deleting the user machine: {ex.Message}"
            );
        }
    }

    public async Task<ApiResponse<UserMachineDto>> GetByIdAsync(Guid id)
    {
        try
        {
            var result = await userMachineRepository.GetAsync(id);

            return await result.MatchAsync(
                userMachine =>
                {
                    var dto = mapper.Map<UserMachineDto>(userMachine);
                    return Task.FromResult(new ApiResponse<UserMachineDto>(
                        StatusCodes.Status200OK,
                        nameof(UserMachine) + ResponseMessages.Get,
                        dto
                    ));
                },
                error =>
                {
                    var statusCode = error switch
                    {
                        EntityNotFound => StatusCodes.Status404NotFound,
                        EntitySoftDeleted => StatusCodes.Status410Gone,
                        _ => StatusCodes.Status400BadRequest
                    };

                    return Task.FromResult(new ApiResponse<UserMachineDto>(
                        statusCode,
                        $"UserMachine with ID {id} not accessible: {error.GetType().Name}"
                    ));
                }
            );
        }
        catch (Exception ex)
        {
            return new ApiResponse<UserMachineDto>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred while retrieving the user machine: {ex.Message}"
            );
        }
    }

    public async Task<ApiResponse<IEnumerable<UserMachineDto>>> GetListAsync(PageParameters pageParameters)
    {
        try
        {
            var termNormalized = pageParameters.Term?.Trim().ToLower() ?? string.Empty;

            Expression<Func<UserMachine, bool>> searchExpr = um =>
                string.IsNullOrEmpty(termNormalized)
                || EF.Functions.Like(um.Machine.MachineName.ToLower(), $"%{termNormalized}%");

            var userMachineFilters = new List<Expression<Func<UserMachine, bool>>>();

            var userMachines = await userMachineRepository.GetListAsync(
                pageParameters,
                searchExpr,
                userMachineFilters,
                q => q.OrderBy(x => x.CreatedAt)
            );

            return new ApiResponse<IEnumerable<UserMachineDto>>(
                StatusCodes.Status200OK,
                nameof(UserMachine) + ResponseMessages.GetAll,
                mapper.Map<IEnumerable<UserMachineDto>>(userMachines)
            );
        }
        catch (Exception ex)
        {
            return new ApiResponse<IEnumerable<UserMachineDto>>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred: {ex.Message}"
            );
        }
    }

    public async Task<ApiResponse<UserMachineDto>> UpdateAsync(Guid id, UpdateUserMachineDto request)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync();

        try
        {
            var result = await userMachineRepository.GetAsync(id);

            return await result.MatchAsync(
                async existing =>
                {
                    if (!await unitOfWork.MachineRepository.ExistsAsyncs(request.MachineId))
                    {
                        return new ApiResponse<UserMachineDto>(
                            StatusCodes.Status200OK,
                            $"Machine with ID {request.MachineId} does not exist."
                        );
                    }

                    mapper.Map(request, existing);

                    var updated = await userMachineRepository.UpdateAsync(id, existing);

                    await unitOfWork.SaveChangesAsync();
                    await transaction.CommitAsync();

                    var dto = mapper.GetResult<UserMachine, UserMachineDto>(updated);
                    return new ApiResponse<UserMachineDto>(
                        StatusCodes.Status200OK,
                        nameof(UserMachine) + ResponseMessages.Updated,
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

                    return Task.FromResult(new ApiResponse<UserMachineDto>(
                        statusCode,
                        $"UserMachine with ID {id} not accessible: {error.GetType().Name}"
                    ));
                }
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new ApiResponse<UserMachineDto>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred: {ex.Message}"
            );
        }
    }
}