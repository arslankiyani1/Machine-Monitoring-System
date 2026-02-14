using MMS.Application.Ports.In.CustomerBilling.Dto;

namespace MMS.Application.Ports.In.CustomerBilling;

public interface ICustomerBillingAddressService
{
    Task<ApiResponse<IEnumerable<CustomerBillingAddressDto>>> GetListAsync(Guid customerId);
    Task<ApiResponse<CustomerBillingAddressDto>> GetByIdAsync(Guid billingAddressId);
    Task<ApiResponse<CustomerBillingAddressDto>> AddAsync(AddCustomerBillingAddressDto request);
    Task<ApiResponse<CustomerBillingAddressDto>> UpdateAsync(Guid billingAddressId, UpdateCustomerBillingAddressDto request);
    Task<ApiResponse<string>> DeleteAsync(Guid billingAddressId);
}
