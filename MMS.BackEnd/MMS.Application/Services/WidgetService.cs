namespace MMS.Application.Services;

public class WidgetService(
    IWidgetRepository widgetRepository,
    AutoMapperResult mapper,
    IUserContextService userContextService,
    IUnitOfWork unitOfWork,
    ICacheService cache) : IWidgetService
{
    private const string WidgetCachePath = "Widget";

    public async Task<ApiResponse<WidgetDto>> AddAsync(AddWidgetDto request)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync();
        try
        {
            var dashboardResult = await unitOfWork.CustomerDashboardRepository.GetAsync(request.DashboardId);
            if (dashboardResult.IsLeft)
            {
                return new ApiResponse<WidgetDto>(
                  StatusCodes.Status200OK,
                 $"Dashboard with ID {request.DashboardId} does not exist."
             );
            }
            var dashboard = dashboardResult.IfRight();
            await CustomerAccessHelper.ValidateCustomerAccessAsync(userContextService, dashboard!.CustomerId);

            var widget = mapper.Map<Widget>(request);
            var addedWidget = await widgetRepository.AddAsync(widget);
            await unitOfWork.SaveChangesAsync();

            // Invalidate related cache
            await cache.RemoveTrackedKeysAsync(WidgetCachePath);

            await transaction.CommitAsync();

            return new ApiResponse<WidgetDto>(
                StatusCodes.Status201Created,
                nameof(Widget) + ResponseMessages.Added, mapper.GetResult<Widget, WidgetDto>(addedWidget)
            );
        }
        catch (UnauthorizedAccessException ex)
        {
            await transaction.RollbackAsync();
            return new ApiResponse<WidgetDto>(
                StatusCodes.Status403Forbidden,
                ex.Message
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new ApiResponse<WidgetDto>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred: {ex.Message}"
            );
        }
    }

    public async Task<ApiResponse<string>> DeleteAsync(Guid widgetId)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync();

        try
        {
            var result = await widgetRepository.GetAsync(widgetId);

            return await result.MatchAsync(
                async widget =>
                {
                    var dashboardResult = await unitOfWork.CustomerDashboardRepository.GetAsync(widget.DashboardId);
                    if (dashboardResult.IsLeft)
                    {
                        return new ApiResponse<string>(
                         StatusCodes.Status400BadRequest,
                         $"Dashboard with ID {widget.DashboardId} does not exist."
                     );
                    }
                    var dashboard = dashboardResult.IfRight();
                    await CustomerAccessHelper.ValidateCustomerAccessAsync(userContextService, dashboard!.CustomerId);

                    await widgetRepository.DeleteAsync(widgetId);
                    await unitOfWork.SaveChangesAsync();
                    await cache.RemoveTrackedKeysAsync(WidgetCachePath);

                    await transaction.CommitAsync();

                    return new ApiResponse<string>(
                        StatusCodes.Status200OK,
                        $"{nameof(Widget)} {ResponseMessages.Deleted}"
                    );
                },
                async error =>
                {
                    return new ApiResponse<string>(
                        StatusCodes.Status200OK,
                        $"Widget with ID {widgetId} not found."
                    );
                }
            );
        }
        catch (UnauthorizedAccessException ex)
        {
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
                $"An error occurred while deleting widget: {ex.Message}"
            );
        }
    }

    public async Task<ApiResponse<WidgetDto>> GetByIdAsync(Guid widgetId)
    {
        try
        {
            var cacheKey = $"widget:{widgetId}";
            var cached = await cache.GetAsync<WidgetDto>(cacheKey);
            if (cached is not null)
            {
                return new ApiResponse<WidgetDto>(
                    StatusCodes.Status200OK,
                    $"{nameof(Widget)} {ResponseMessages.cache}",
                    cached
                );
            }

            var result = await widgetRepository.GetAsync(widgetId);

            return await result.MatchAsync(
               async right =>
               {
                   var dashboardResult = await unitOfWork.CustomerDashboardRepository.GetAsync(right.DashboardId);
                   if (dashboardResult.IsLeft)
                   {
                       return new ApiResponse<WidgetDto>(
                        StatusCodes.Status400BadRequest,
                        $"Dashboard with ID {right.DashboardId} does not exist."
                    );
                   }
                   var dashboard = dashboardResult.IfRight();
                   await CustomerAccessHelper.ValidateCustomerAccessAsync(userContextService, dashboard!.CustomerId);

                   var dto = mapper.Map<WidgetDto>(right);
                   await cache.SetAsync(cacheKey, dto, TimeSpan.FromHours(1), WidgetCachePath);

                   return new ApiResponse<WidgetDto>(
                       StatusCodes.Status200OK,
                       $"{nameof(Widget)} {ResponseMessages.Get}",
                       dto
                   );
               },
                async left => {
                    return new ApiResponse<WidgetDto>(
                         StatusCodes.Status404NotFound,
                         left.Message
                     );
                }
            );
        }
        catch (UnauthorizedAccessException ex)
        {
            return new ApiResponse<WidgetDto>(
                StatusCodes.Status403Forbidden,
                ex.Message
            );
        }
        catch (Exception ex)
        {
            return new ApiResponse<WidgetDto>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred: {ex.Message}"
            );
        }
    }

    public async Task<ApiResponse<IEnumerable<WidgetDto>>> GetListAsync(
        PageParameters pageParameters,
        WidgetType? widgetType,
        WidgetSourceType? SourceType)
    {
        try
        {
            var cacheKey = $"widget:list:{widgetType}:{SourceType}:{pageParameters.Skip}:{pageParameters.Top}:{pageParameters.Term}";
            var cached = await cache.GetAsync<IEnumerable<WidgetDto>>(cacheKey);
            if (cached is not null)
            {
                return new ApiResponse<IEnumerable<WidgetDto>>(
                    StatusCodes.Status200OK,
                    $"{nameof(Widget)} {ResponseMessages.cache}",
                    cached
                );
            }

            var termNormalized = pageParameters.Term?.Trim().ToLower() ?? string.Empty;

            Expression<Func<Widget, bool>> searchExpr = _ => true;

            var widgetFilters = new List<Expression<Func<Widget, bool>>>();

            var allowedDashboardIds = await GetAllowedDashBoardIdsAsync();

            if (widgetType != default)
                widgetFilters.Add(c => c.WidgetType == widgetType);

            if (SourceType != default)
                widgetFilters.Add(c => c.SourceType == SourceType);

            if (allowedDashboardIds.Any())
            {
                widgetFilters.Add(t => allowedDashboardIds.Contains(t.DashboardId));
            }
            else
            {
                return new ApiResponse<IEnumerable<WidgetDto>>(
                    StatusCodes.Status200OK,
                    nameof(Widget) + ResponseMessages.GetAll,
                    Enumerable.Empty<WidgetDto>()
                );
            }

            var widgets = await widgetRepository.GetListAsync(
                pageParameters,
                searchExpr,
                widgetFilters,
                q => q.OrderBy(w => w.WidgetType)
            );

            var mapped = mapper.Map<IEnumerable<WidgetDto>>(widgets);
            await cache.SetAsync(cacheKey, mapped, TimeSpan.FromHours(1), WidgetCachePath);

            return new ApiResponse<IEnumerable<WidgetDto>>(
                StatusCodes.Status200OK,
                nameof(Widget) + ResponseMessages.GetAll,
                mapped
            );
        }
        catch (UnauthorizedAccessException ex)
        {
            return new ApiResponse<IEnumerable<WidgetDto>>(
                StatusCodes.Status403Forbidden,
                ex.Message
            );
        }
        catch (Exception ex)
        {
            return new ApiResponse<IEnumerable<WidgetDto>>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred: {ex.Message}"
            );
        }
    }

    public async Task<ApiResponse<WidgetDto>> UpdateAsync(Guid widgetId, UpdateWidgetDto request)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync();

        try
        {
            var result = await widgetRepository.GetAsync(widgetId);

            return await result.MatchAsync(
                async widget =>
                {
                    var dashboardResult = await unitOfWork.CustomerDashboardRepository.GetAsync(request.DashboardId);
                    if (dashboardResult.IsLeft)
                    {
                        return new ApiResponse<WidgetDto>(
                            StatusCodes.Status404NotFound,
                         $"Dashboard with ID {request.DashboardId} does not exist."
                     );
                    }
                    var dashboard = dashboardResult.IfRight();
                    await CustomerAccessHelper.ValidateCustomerAccessAsync(userContextService, dashboard!.CustomerId);

                    mapper.Map(request, widget);

                    var updatedWidget = await widgetRepository.UpdateAsync(widgetId, widget);
                    await unitOfWork.SaveChangesAsync();

                    // Invalidate cache
                    await cache.RemoveTrackedKeysAsync(WidgetCachePath);

                    await transaction.CommitAsync();

                    return new ApiResponse<WidgetDto>(
                        StatusCodes.Status200OK,
                        $"{nameof(Widget)} {ResponseMessages.Updated}",
                        mapper.GetResult<Widget, WidgetDto>(updatedWidget)
                    );
                },
                left =>
                {
                    return Task.FromResult(new ApiResponse<WidgetDto>(
                        StatusCodes.Status404NotFound,
                        left.Message
                    ));
                }
            );
        }
        catch (UnauthorizedAccessException ex)
        {
            await transaction.RollbackAsync();
            return new ApiResponse<WidgetDto>(
                StatusCodes.Status403Forbidden,
                ex.Message
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new ApiResponse<WidgetDto>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred: {ex.Message}"
            );
        }
    }

    private async Task<List<Guid>> GetAllowedDashBoardIdsAsync()
    {
        try
        {
            Expression<Func<CustomerDashboard, bool>> dashSearchExpr = m => true;
            var dashFilters = new List<Expression<Func<CustomerDashboard, bool>>>();

            var customerIds = CustomerAccessHelper.GetAccessibleCustomerIds(userContextService);
            if (customerIds != null && customerIds.Any())
            {
                dashFilters.Add(m => customerIds.Contains(m.CustomerId));
            }

            var cusDashs = await unitOfWork.CustomerDashboardRepository.GetListAsync(
                null,
                dashSearchExpr,
                dashFilters,
                q => q.OrderBy(m => m.Id)
            );

            return cusDashs.Select(m => m.Id).ToList();
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to fetch allowed Dashboard IDs: {ex.Message}", ex);
        }
    }
}
