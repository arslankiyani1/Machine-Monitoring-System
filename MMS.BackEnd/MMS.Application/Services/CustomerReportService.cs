namespace MMS.Application.Services;

public class CustomerReportService(
    ICustomerReportRepository reportRepository,
    AutoMapperResult mapper,
    IUserContextService userContextService,
    IUnitOfWork unitOfWork,
    ICacheService cache
) : ICustomerReportService
{
    private const string CachePrefix = "CustomerReport:";
    private string GetCacheKey(Guid id) => $"{CachePrefix}{id}:{userContextService.UserId}";

    public async Task<ApiResponse<CustomerReportDto>> AddAsync(AddCustomerReportDto request)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync();

        try
        {
            await CustomerAccessHelper.ValidateCustomerAccessAsync(userContextService, request.CustomerId);

            var report = mapper.Map<CustomerReport>(request);
            var result = await reportRepository.AddAsync(report);

            await unitOfWork.SaveChangesAsync();
            await cache.RemoveTrackedKeysAsync(CachePrefix);
            await transaction.CommitAsync();

            return new ApiResponse<CustomerReportDto>(
                StatusCodes.Status201Created,
                nameof(CustomerReport) + ResponseMessages.Added,
                mapper.GetResult<CustomerReport, CustomerReportDto>(result)
            );
        }
        catch (UnauthorizedAccessException ex)
        {
            await transaction.RollbackAsync();
            return new ApiResponse<CustomerReportDto>(StatusCodes.Status403Forbidden, ex.Message, null);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new ApiResponse<CustomerReportDto>(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}", null);
        }
    }

    public async Task<ApiResponse<string>> DeleteAsync(Guid id)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync();

        try
        {
            var result = await reportRepository.GetAsync(id);

            return await result.MatchAsync(
                async report =>
                {
                    await CustomerAccessHelper.ValidateCustomerAccessAsync(userContextService, report.CustomerId);
                    await reportRepository.DeleteAsync(id);
                    await unitOfWork.SaveChangesAsync();

                    await cache.RemoveTrackedKeysAsync(CachePrefix);
                    await transaction.CommitAsync();

                    return new ApiResponse<string>(StatusCodes.Status200OK, nameof(CustomerReport) + ResponseMessages.Deleted, null);
                },
                error =>
                {
                    var status = error switch
                    {
                        EntityNotFound => StatusCodes.Status404NotFound,
                        EntitySoftDeleted => StatusCodes.Status410Gone,
                        _ => StatusCodes.Status400BadRequest
                    };
                    return Task.FromResult(new ApiResponse<string>(status, $"CustomerReport not accessible: {error.GetType().Name}", null));
                });
        }
        catch (UnauthorizedAccessException ex)
        {
            await transaction.RollbackAsync();
            return new ApiResponse<string>(StatusCodes.Status403Forbidden, ex.Message, null);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new ApiResponse<string>(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}", null);
        }
    }

    public async Task<ApiResponse<CustomerReportDto>> GetByIdAsync(Guid id)
    {
        try
        {
            var cacheKey = GetCacheKey(id);
            var cached = await cache.GetAsync<CustomerReportDto>(cacheKey);
            if (cached is not null)
                return new ApiResponse<CustomerReportDto>(StatusCodes.Status200OK, "Fetched from cache", cached);

            var result = await reportRepository.GetAsync(id);

            return await result.MatchAsync(
                async report =>
                {
                    await CustomerAccessHelper.ValidateCustomerAccessAsync(userContextService, report.CustomerId);
                    var dto = mapper.Map<CustomerReportDto>(report);
                    await cache.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(30), CachePrefix);
                    return new ApiResponse<CustomerReportDto>(StatusCodes.Status200OK, nameof(CustomerReport) + ResponseMessages.Get, dto);
                },
                error =>
                {
                    var status = error switch
                    {
                        EntityNotFound => StatusCodes.Status404NotFound,
                        EntitySoftDeleted => StatusCodes.Status410Gone,
                        _ => StatusCodes.Status400BadRequest
                    };
                    return Task.FromResult(new ApiResponse<CustomerReportDto>(status, error.Message, null));
                });
        }
        catch (Exception ex)
        {
            return new ApiResponse<CustomerReportDto>(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}", null);
        }
    }


    public async Task<ApiResponse<IEnumerable<CustomerReportDto>>> GetListAsync(
    PageParameters pageParameters,
    ReportType? reportType,
    bool? isSent,
    ReportFrequency? reportFrequency,
    Guid? customerId)
    {
        try
        {
            Expression<Func<CustomerReport, bool>> searchExpr = _ => true;
            var filters = new List<Expression<Func<CustomerReport, bool>>>();

            if (reportType.HasValue)
                filters.Add(r => r.CustomerReportSetting.ReportType.Contains(reportType.Value));

            if (reportFrequency.HasValue)
                filters.Add(r => r.CustomerReportSetting.Frequency == reportFrequency.Value);

            if (isSent.HasValue)
                filters.Add(r => r.IsSent == isSent.Value);

            var accessibleCustomerIds = CustomerAccessHelper.GetAccessibleCustomerIds(userContextService);

            if (customerId.HasValue && customerId.Value != Guid.Empty)
            {
                if (accessibleCustomerIds == null || accessibleCustomerIds.Contains(customerId.Value))
                {
                    // ✅ Allowed: filter only by this customer
                    filters.Add(r => r.CustomerId == customerId.Value);
                }
                else
                {
                    // ❌ Not allowed
                    return new ApiResponse<IEnumerable<CustomerReportDto>>(
                        StatusCodes.Status403Forbidden,
                        "You are not authorized to access this customer’s reports."
                    );
                }
            }
            else
            {
                if (accessibleCustomerIds != null && accessibleCustomerIds.Any())
                {
                    // ✅ Restrict to allowed customers
                    filters.Add(r => accessibleCustomerIds.Contains(r.CustomerId));
                }
                // else: no filtering → all customers are visible
            }

            var reports = await reportRepository.GetListAsync(
                pageParameters,
                searchExpr,
                filters,
                q => q.OrderByDescending(x => x.GeneratedDate));

            var mapped = mapper.Map<IEnumerable<CustomerReportDto>>(reports);

            return new ApiResponse<IEnumerable<CustomerReportDto>>(
                StatusCodes.Status200OK,
                nameof(CustomerReport) + ResponseMessages.GetAll,
                mapped);
        }
        catch (Exception ex)
        {
            return new ApiResponse<IEnumerable<CustomerReportDto>>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred: {ex.Message}"
            );
        }
    }


}
