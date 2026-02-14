namespace MMS.Adapters.PostgreSQL.Repositories;

public class CustomerDashboardRepository(
    ApplicationDbContext dbContext,
    ILogger<CustomerDashboardRepository> logger) : MMsCrudRepository<CustomerDashboard>(dbContext, logger), ICustomerDashboardRepository
{
    public async Task DeleteByCustomerIdAsync(Guid customerId)
    {
        var dashboards = await dbContext.CustomerDashboards
            .Where(cd => cd.CustomerId == customerId)
            .ToListAsync();

        dbContext.CustomerDashboards.RemoveRange(dashboards);
    }

    public async Task<bool> ExistsAsync(Guid? dashboardId)
    {
        return await dbContext.CustomerDashboards.AnyAsync(c => c.Id == dashboardId);
    }

    public async Task<bool> ExistsByCustomerIdAsync(Guid? customerId)
    {
        return await dbContext.CustomerDashboards.AnyAsync(c => c.CustomerId == customerId);
    }
}