namespace MMS.Adapters.PostgreSQL.Repositories;

public class MachineSensorRepository(
        ApplicationDbContext dbContext,
        ILogger<MachineSensorRepository> logger
    ) : MMsCrudRepository<MachineSensor>(dbContext, logger), IMachineSensorRepository
{
    public async Task<bool> ExistsAsync(Guid? sensorId)
    {
        return await dbContext.MachineSensor
            .AsNoTracking()
            .AnyAsync(s => s.Id == sensorId);
    }

    public async Task<List<MachineSensor>> GetByCustomerIdAsync(Guid customerId)
    {
        return await dbContext.MachineSensor
            .AsNoTracking()
            .Where(s => s.CustomerId == customerId)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<MachineSensor?> GetByNameAsync(string sensorName)
    {
        return await dbContext.MachineSensor
       .AsNoTracking()
       .FirstOrDefaultAsync(s => s.Name.ToLower() == sensorName.Trim().ToLower());
    }
}
