
namespace MMS.Application.Ports.In.NoSql.CustomerDashSummary;

public interface ICustomerDashboardSummaryService
{
    Task<ApiResponse<IEnumerable<CustomerDashboardSummary>>> GetAllAsync();
    Task<ApiResponse<CustomerDashboardSummary>> GetByCustomerIdAsync(string customerId);
    Task<ApiResponse<CustomerDashboardSummary>> UpsertAsync(CustomerDashboardSummary entity);
    Task<ApiResponse<CustomerDashboardSummary>> UpsertCustomerDashboardSummaryAsync(MachineLogSignalRDto entity);

    Task<ApiResponse<string>> DeleteAsync(string customerId);
}