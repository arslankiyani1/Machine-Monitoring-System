namespace MMS.Adapters.PostgreSQL.Repositories;

public class MachineSettingRepository(
    ApplicationDbContext dbContext,
    ILogger<MachineSettingRepository> logger) : MMsCrudRepository<MachineSetting>(dbContext, logger), IMachineSettingRepository
{
    public async Task<bool> ExistsByMachineIdAsync(Guid machineId)
    {
        return await dbContext.MachineSettings
            .AsNoTracking()
            .AnyAsync(ms => ms.MachineId == machineId);
    }

    public async Task<MachineSetting?> GetByMachineIdAsync(Guid machineId)
    {
        return await dbContext.MachineSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(ms => ms.MachineId == machineId);
    }
}
