namespace MMS.Application.Ports.Out.Persistence.Interfaces.NoSql;

public interface ICustomerDashboardSummaryRepository
{
    Task<IEnumerable<CustomerDashboardSummary>> GetAllAsync(IEnumerable<string>? customerIds = null);
    Task<CustomerDashboardSummary?> GetByCustomerIdAsync(string customerId);
    Task UpsertAsync(CustomerDashboardSummary entity);
    Task DeleteAsync(string customerId);
}