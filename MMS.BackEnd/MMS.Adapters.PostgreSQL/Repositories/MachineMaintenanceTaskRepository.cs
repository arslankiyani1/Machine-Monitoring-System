namespace MMS.Adapters.PostgreSQL.Repositories;

public class MachineMaintenanceTaskRepository(
    ApplicationDbContext dbContext,
    ILogger<MachineMaintenanceTaskRepository> logger
) : MMsCrudRepository<MachineMaintenanceTask>(dbContext, logger), IMachineMaintenanceTaskRepository
{
    public async Task DeleteByCustomerIdAsync(Guid machieId)
    {

        var machines = await dbContext.MachineMaintenances
        .Where(m => m.MachineId == machieId)
        .ToListAsync();

        dbContext.MachineMaintenances.RemoveRange(machines);
    }

    public async Task DeleteByMachineIdAsync(Guid machineId)
    {
        var entities = await dbContext.MachineMaintenances
        .Where(x => x.MachineId == machineId)
        .ToListAsync();

        dbContext.MachineMaintenances.RemoveRange(entities);
    }
    

    public async Task<bool> ExistsAsync(Guid taskId)
    {
        return await dbContext.MachineMaintenances.AnyAsync(x => x.Id == taskId);
    }
}