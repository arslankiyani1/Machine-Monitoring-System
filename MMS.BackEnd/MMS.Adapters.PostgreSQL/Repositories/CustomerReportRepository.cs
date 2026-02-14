namespace MMS.Adapters.PostgreSQL.Repositories;

public class CustomerReportRepository(ApplicationDbContext dbContext, ILogger<CustomerReportRepository> logger)
    : MMsCrudRepository<CustomerReport>(dbContext, logger), ICustomerReportRepository
{
    public async Task<bool> ExistsAsync(Guid id)
    {
        return await dbContext.CustomerReportSetting.AnyAsync(x => x.Id == id);
    }
}
