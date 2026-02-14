namespace MMS.Application.Ports.In.CustomerSubscription;

public interface ICustomerSubscriptionService
{
    Task<ApiResponse<IEnumerable<CustomerSubscriptionDto>>> GetByCustomerIdAsync(Guid customerId);
    Task<ApiResponse<IEnumerable<CustomerSubscriptionDto>>> GetListAsync(PageParameters pageParameters,
        Guid Id, CustomerSubscriptionStatus? status,DateTime? start,DateTime? end);
    Task<ApiResponse<CustomerSubscriptionDto>> GetByIdAsync(Guid id);
    Task<ApiResponse<CustomerSubscriptionDto>> AddAsync(AddCustomerSubscriptionDto request);
    Task<ApiResponse<CustomerSubscriptionDto>> UpdateAsync(UpdateCustomerSubscriptionDto request);
    Task<ApiResponse<string>> DeleteAsync(Guid id);
}
