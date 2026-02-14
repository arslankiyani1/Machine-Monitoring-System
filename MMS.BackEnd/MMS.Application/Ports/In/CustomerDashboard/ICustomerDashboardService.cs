namespace MMS.Application.Ports.In.CustomerDashboard;

public interface ICustomerDashboardService
{
    Task<ApiResponse<IEnumerable<CustomerDashboardDto>>> GetListAsync(PageParameters pageParameters, DashboardStatus? dashboardStatus, Guid CustomerId);
    Task<ApiResponse<CustomerDashboardDto>> GetByIdAsync(Guid dashboardId);
    Task<ApiResponse<CustomerDashboardDto>> AddAsync(AddCustomerDashboardDto request);
    Task<ApiResponse<CustomerDashboardDto>> UpdateAsync(Guid dashboardId, UpdateCustomerDashboardDto request);
    Task<ApiResponse<string>> DeleteAsync(Guid dashboardId);
}
