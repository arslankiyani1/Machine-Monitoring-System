namespace MMS.Application.Ports.In.Subscription;

public interface ISubscriptionService
{
    Task<ApiResponse<IEnumerable<SubscriptionDto>>> GetListAsync(PageParameters pageParameters,SubscriptionStatus? status);
    Task<ApiResponse<SubscriptionDto>> GetByIdAsync(Guid subscriptionId);
    Task<ApiResponse<SubscriptionDto>> AddAsync(SubscriptionAddDto request);
    Task<ApiResponse<SubscriptionDto>> UpdateAsync(Guid subscriptionId, SubscriptionUpdateDto request);
    Task<ApiResponse<string>> DeleteAsync(Guid subscriptionId);

    Task<ApiResponse<string>> ProcessSubscriptionPaymentAsync(SubscriptionPaymentDto dto);
}