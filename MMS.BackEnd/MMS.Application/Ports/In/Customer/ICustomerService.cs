namespace MMS.Application.Ports.In.Customer;

public interface ICustomerService
{
    Task<ApiResponse<CustomerDto>> DeleteAsync(Guid customerId);
    Task<ApiResponse<IEnumerable<CustomerDto>>> GetListAsync(PageParameters  pageParameters,CustomerStatus? customerStatus);
    Task<ApiResponse<CustomerDto>> GetByIdAsync(Guid customerId);
    Task<ApiResponse<CustomerDto>> AddAsync(AddCustomerDto request);
    Task<ApiResponse<CustomerDto>> UpdateAsync(Guid customerId, UpdateCustomerDto request);
    Task<ApiResponse<List<CustomerCardDto>>> GetCustomerDashboardAsync(PageParameters pageParameters, Guid? customerId);
    Task<ApiResponse<List<MachineJobDto>>> GetMachinesByCustomerIdAsync(Guid customerId, 
        PageParameters pageParameters, DateTime from, DateTime to);
    Task<ApiResponse<CustomerCardDto>> GetCustomerSummaryAsync(Guid customerId);
    Task<ApiResponse<List<CustomerCardDto>>> GetCustomerDashboardDetailsAsync(PageParameters pageParameters);

}