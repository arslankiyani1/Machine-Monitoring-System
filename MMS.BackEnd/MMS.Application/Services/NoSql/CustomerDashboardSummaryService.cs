namespace MMS.Application.Services.NoSql;

public class CustomerDashboardSummaryService(
    ICustomerDashboardSummaryRepository repository,
    IUserContextService userContextService,
    IUnitOfWork unitOfWork,
    ICacheService cacheService) : ICustomerDashboardSummaryService
{
    
    private const string CachePrefix = "CustomerDashboardSummary";

    public async Task<ApiResponse<IEnumerable<CustomerDashboardSummary>>> GetAllAsync()
    {
        try
        {
            var allowedCustomerIds = GetAllowedCustomerIds();
            var items = await repository.GetAllAsync(allowedCustomerIds);


            return new ApiResponse<IEnumerable<CustomerDashboardSummary>>(
                StatusCodes.Status200OK,
                $"{nameof(CustomerDashboardSummary)} {ResponseMessages.GetAll}",
                items
            );
        }
        catch (UnauthorizedAccessException ex)
        {
            return new ApiResponse<IEnumerable<CustomerDashboardSummary>>(StatusCodes.Status403Forbidden, ex.Message);
        }
        catch (Exception ex)
        {
            return new ApiResponse<IEnumerable<CustomerDashboardSummary>>(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
        }
    }

    public async Task<ApiResponse<CustomerDashboardSummary>> GetByCustomerIdAsync(string customerId)
    {
        try
        {
            var customerIdGuid = Guid.Parse(customerId);
            var customerResult = await unitOfWork.CustomerRepository.GetAsync(customerIdGuid);
            if (customerResult.IsLeft)
                return new ApiResponse<CustomerDashboardSummary>(StatusCodes.Status404NotFound, $"Customer with ID {customerId} not found.");

            var customer = customerResult.IfRight();
            await CustomerAccessHelper.ValidateCustomerAccessAsync(userContextService, customer!.Id);

            var item = await repository.GetByCustomerIdAsync(customerId);
            if (item == null)
                return new ApiResponse<CustomerDashboardSummary>(StatusCodes.Status204NoContent, $"Entity with customer ID {customerId} not found.");

            return new ApiResponse<CustomerDashboardSummary>(StatusCodes.Status200OK, $"{nameof(CustomerDashboardSummary)} {ResponseMessages.Get}", item);
        }
        catch (UnauthorizedAccessException ex)
        {
            return new ApiResponse<CustomerDashboardSummary>(StatusCodes.Status403Forbidden, ex.Message);
        }
        catch (Exception ex)
        {
            return new ApiResponse<CustomerDashboardSummary>(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
        }
    }

    public async Task<ApiResponse<CustomerDashboardSummary>> UpsertAsync(CustomerDashboardSummary entity)
    {
        try
        {
            if (entity == null || string.IsNullOrEmpty(entity.CustomerId))
                return new ApiResponse<CustomerDashboardSummary>(StatusCodes.Status400BadRequest, "Invalid input data.");

            await repository.UpsertAsync(entity);

            await cacheService.RemoveAsync($"{CachePrefix}:{entity.CustomerId}");
            await cacheService.RemoveAsync($"{CachePrefix}:All");

            return new ApiResponse<CustomerDashboardSummary>(StatusCodes.Status200OK, "Upserted successfully.", entity);
        }
        catch (UnauthorizedAccessException ex)
        {
            return new ApiResponse<CustomerDashboardSummary>(StatusCodes.Status403Forbidden, ex.Message);
        }
        catch (Exception ex)
        {
            return new ApiResponse<CustomerDashboardSummary>(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
        }
    }

    public async Task<ApiResponse<CustomerDashboardSummary>> UpsertCustomerDashboardSummaryAsync(MachineLogSignalRDto dto)
    {
        try
        {
            CustomerDashboardSummary entity = new()
            {
                CustomerId = dto.CustomerId.ToString(),
                Id = dto.CustomerId.ToString(),
                StatusSummary = dto.StatusSummary,
            };
            if (entity == null || string.IsNullOrEmpty(entity.CustomerId))
                return new ApiResponse<CustomerDashboardSummary>(StatusCodes.Status400BadRequest, "Invalid input data.");

            await repository.UpsertAsync(entity);

            return new ApiResponse<CustomerDashboardSummary>(StatusCodes.Status200OK, "Upserted successfully.", entity);
        }
        catch (UnauthorizedAccessException ex)
        {
            return new ApiResponse<CustomerDashboardSummary>(StatusCodes.Status403Forbidden, ex.Message);
        }
        catch (Exception ex)
        {
            return new ApiResponse<CustomerDashboardSummary>(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
        }
    }

    public async Task<ApiResponse<string>> DeleteAsync(string customerId)
    {
        try
        {
            var customerIdGuid = Guid.Parse(customerId);
            var customerResult = await unitOfWork.CustomerRepository.GetAsync(customerIdGuid);
            if (customerResult.IsLeft)
                return new ApiResponse<string>(StatusCodes.Status404NotFound, "Customer not found.");

            var customer = customerResult.IfRight();
            await CustomerAccessHelper.ValidateCustomerAccessAsync(userContextService, customer!.Id);

            var existing = await repository.GetByCustomerIdAsync(customerId);
            if (existing == null)
                return new ApiResponse<string>(StatusCodes.Status204NoContent, $"Entity with customer ID {customerId} not found.");

            await repository.DeleteAsync(customerId);

            await cacheService.RemoveAsync($"{CachePrefix}:{customerId}");
            await cacheService.RemoveAsync($"{CachePrefix}:All");

            return new ApiResponse<string>(StatusCodes.Status200OK, "Deleted successfully.");
        }
        catch (UnauthorizedAccessException ex)
        {
            return new ApiResponse<string>(StatusCodes.Status403Forbidden, ex.Message);
        }
        catch (Exception ex)
        {
            return new ApiResponse<string>(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
        }
    }

    private List<string> GetAllowedCustomerIds()
    {
        try
        {
            var customerIds = CustomerAccessHelper.GetAccessibleCustomerIds(userContextService);
            return customerIds!.Select(c => c.ToString()).ToList();
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to fetch allowed customer IDs: {ex.Message}", ex);
        }
    }
}