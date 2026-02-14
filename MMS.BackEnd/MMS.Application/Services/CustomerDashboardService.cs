namespace MMS.Application.Services;

public class CustomerDashboardService(
    ICustomerDashboardRepository dashboardRepository,
    AutoMapperResult mapper,
    IUserContextService userContextService,
    IUnitOfWork unitOfWork,
    ICacheService cache) : ICustomerDashboardService
{
    public async Task<ApiResponse<CustomerDashboardDto>> AddAsync(AddCustomerDashboardDto request)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync();
        try
        {
            await CustomerAccessHelper.ValidateCustomerAccessAsync(userContextService, request.CustomerId);

            var existingCustomerResult = await unitOfWork.CustomerRepository.GetAsync(request.CustomerId);

            if (!await unitOfWork.CustomerRepository.ExistsAsync(request.CustomerId))
            {
                return new ApiResponse<CustomerDashboardDto>(
                    StatusCodes.Status400BadRequest,
                    $"Customer with ID {request.CustomerId} does not exist."
                );
            }
            if (await unitOfWork.CustomerDashboardRepository.ExistsByCustomerIdAsync(request.CustomerId))
            {
                return new ApiResponse<CustomerDashboardDto>(
                    StatusCodes.Status409Conflict,
                    $"Cannot create a new dashboard. {request.CustomerId} One already exists for this customer."
                );
            }
            var dashboard = mapper.Map<CustomerDashboard>(request);
            await unitOfWork.CustomerDashboardRepository.AddAsync(dashboard);
            await unitOfWork.SaveChangesAsync();

            await cache.RemoveTrackedKeysAsync(CustomerDashboardCachePath);
            await transaction.CommitAsync();

            return new ApiResponse<CustomerDashboardDto>(StatusCodes.Status201Created,
                nameof(CustomerDashboard) + ResponseMessages.Added, 
                mapper.GetResult<CustomerDashboard, CustomerDashboardDto>(dashboard));
        }
        catch (UnauthorizedAccessException ex)
        {
            await transaction.RollbackAsync();
            return new ApiResponse<CustomerDashboardDto>(
                StatusCodes.Status403Forbidden,
                ex.Message
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new ApiResponse<CustomerDashboardDto>(StatusCodes.Status500InternalServerError, "An error occurred while adding the dashboard: " + ex.Message);
        }
    }

    public async Task<ApiResponse<string>> DeleteAsync(Guid dashboardId)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync();

        try
        {
            var result = await unitOfWork.CustomerDashboardRepository.GetAsync(dashboardId);

            return await result.MatchAsync(
                async dashboard =>
                {
                    await CustomerAccessHelper.ValidateCustomerAccessAsync(userContextService, dashboard.CustomerId);
                    await unitOfWork.CustomerDashboardRepository.DeleteAsync(dashboard.Id);
                    await unitOfWork.SaveChangesAsync();

                    await cache.RemoveTrackedKeysAsync(CustomerDashboardCachePath);
                    await transaction.CommitAsync();

                    return new ApiResponse<string>(
                        StatusCodes.Status200OK,
                        nameof(CustomerDashboard) + ResponseMessages.Deleted,
                        null
                    );
                },
                error =>
                {
                    var status = error switch
                    {
                        EntitySoftDeleted => StatusCodes.Status410Gone,
                        EntityNotFound => StatusCodes.Status404NotFound,
                        _ => StatusCodes.Status400BadRequest
                    };

                    return Task.FromResult(new ApiResponse<string>(
                        status,
                        $"CustomerDashboard with ID {dashboardId} not accessible: {error.GetType().Name}",
                        null
                    ));
                });
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
                "An error occurred while deleting the dashboard: " + ex.Message,
                null
            );
        }
    }

    public async Task<ApiResponse<CustomerDashboardDto>> GetByIdAsync(Guid dashboardId)
    {
        try
        {
            var cacheKey = GetCustomerDashboardKey(dashboardId);
            var cached = await cache.GetAsync<CustomerDashboardDto>(cacheKey);
            if (cached is not null)
            {
                return new ApiResponse<CustomerDashboardDto>(
                    StatusCodes.Status200OK,
                    nameof(Customer) + ResponseMessages.Get,
                    cached
                );
            }
            var result = await dashboardRepository.GetAsync(dashboardId);

            return await result.Match<Task<ApiResponse<CustomerDashboardDto>>>(
                async success =>
                {
                    await CustomerAccessHelper.ValidateCustomerAccessAsync(userContextService, success.CustomerId);

                    return new ApiResponse<CustomerDashboardDto>(
                        StatusCodes.Status200OK,
                        nameof(CustomerDashboard) + ResponseMessages.Get,
                        mapper.Map<CustomerDashboardDto>(success)
                    );
                },
                error => Task.FromResult(new ApiResponse<CustomerDashboardDto>(
                    StatusCodes.Status404NotFound,
                    error.Message ?? "Dashboard not found.")
                )
            );
        }
        catch (Exception ex)
        {
            return new ApiResponse<CustomerDashboardDto>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred: {ex.Message}"
            );
        }
    }

    public async Task<ApiResponse<IEnumerable<CustomerDashboardDto>>> GetListAsync(
        PageParameters pageParameters, 
        DashboardStatus? dashboardStatus, 
        Guid customerID)
    {
        try
        {

            string cacheKey = $"customerdashboard:list:{customerID}:{pageParameters.Term}:{pageParameters.Top}:{pageParameters.Skip}{userContextService.UserId}";
            var cached = await cache.GetAsync<List<CustomerDashboardDto>>(cacheKey);
            if (cached is not null)
            {
                return new ApiResponse<IEnumerable<CustomerDashboardDto>>(StatusCodes.Status200OK, "Fetched from cache", cached);
            }

            var termNormalized = pageParameters.Term?.Trim().ToLower() ?? string.Empty;

            Expression<Func<CustomerDashboard, bool>> searchExpr = d =>
                string.IsNullOrEmpty(termNormalized)
                || EF.Functions.Like(d.Name.ToLower(),$"%{termNormalized}%");

            var dashboardFilters = new List<Expression<Func<CustomerDashboard, bool>>>();
            if (customerID == Guid.Empty)
            {
                var customerIds = CustomerAccessHelper.GetAccessibleCustomerIds(userContextService);
                if (customerIds != null && customerIds.Any())
                {
                    dashboardFilters.Add(d => customerIds.Contains(d.CustomerId));
                }
            }
            else
            {
                await CustomerAccessHelper.ValidateCustomerAccessAsync(userContextService, customerID);
                dashboardFilters.Add(d => d.CustomerId == customerID);
            }

            if (dashboardStatus.HasValue)
            {
                dashboardFilters.Add(d => d.Status == dashboardStatus);
            }
            var dashboards = await dashboardRepository.GetListAsync(
                pageParameters,
                searchExpr,
                dashboardFilters,
                q => q.OrderBy(d => d.Name)
            );
            await cache.SetAsync(cacheKey, dashboards, TimeSpan.FromMinutes(30), CustomerDashboardCachePath);

            return new ApiResponse<IEnumerable<CustomerDashboardDto>>(
                StatusCodes.Status200OK,
                $"{nameof(CustomerDashboard)} {ResponseMessages.GetAll}",
                mapper.Map<IEnumerable<CustomerDashboardDto>>(dashboards)
            );
        }
        catch (UnauthorizedAccessException ex)
        {
            return new ApiResponse<IEnumerable<CustomerDashboardDto>>(
                StatusCodes.Status403Forbidden,
                ex.Message
            );
        }
        catch (Exception ex)
        {
            return new ApiResponse<IEnumerable<CustomerDashboardDto>>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred: {ex.Message}"
            );
        }
    }

    public async Task<ApiResponse<CustomerDashboardDto>> UpdateAsync(Guid dashboardId, UpdateCustomerDashboardDto request)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync();
        try
        {
            var existingCustomerResult = await unitOfWork.CustomerRepository.GetAsync(request.CustomerId);

            var existingResult = await unitOfWork.CustomerDashboardRepository.GetAsync(dashboardId);
            return await existingResult.MatchAsync(
            async dashboard =>
            {
                if ( request.CustomerId.Equals(dashboard.CustomerId) &&
                    !await unitOfWork.CustomerRepository.ExistsAsync(request.CustomerId))
                {
                    return new ApiResponse<CustomerDashboardDto>(
                        StatusCodes.Status200OK,
                        $"Customer with ID {request.CustomerId} not found."
                    );
                }

                mapper.Map(request, dashboard);

                await unitOfWork.SaveChangesAsync();
                await cache.RemoveTrackedKeysAsync(CustomerDashboardCachePath);

                await transaction.CommitAsync();
                return new ApiResponse<CustomerDashboardDto>(
                    StatusCodes.Status200OK,
                    nameof(CustomerDashboard) + ResponseMessages.Updated,
                    mapper.GetResult<CustomerDashboard, CustomerDashboardDto>(dashboard)
                );
            },
            error =>
            {
                var status = error switch
                {
                    EntitySoftDeleted => StatusCodes.Status410Gone,
                    EntityNotFound => StatusCodes.Status404NotFound,
                    _ => StatusCodes.Status400BadRequest
                };

                return Task.FromResult(new ApiResponse<CustomerDashboardDto>(
                    status,
                    $"CustomerDashboard with ID {dashboardId} not accessible: {error.GetType().Name}"
                ));
            });

        }
        catch (UnauthorizedAccessException ex)
        {
            await transaction.RollbackAsync();
            return new ApiResponse<CustomerDashboardDto>(
                StatusCodes.Status403Forbidden,
                ex.Message
            );
        }
        catch (Exception ex) 
        {
            await transaction.RollbackAsync();
            return new ApiResponse<CustomerDashboardDto>(
                StatusCodes.Status500InternalServerError,
                "An error occurred while updating the dashboard: " + ex.Message
            );
        }
    }

    private const string CustomerDashboardCachePath = "CustomerDashboard";
    private string GetCustomerDashboardKey(Guid id) => $"CustomerDashboard:{id}/{userContextService.UserId}";
}