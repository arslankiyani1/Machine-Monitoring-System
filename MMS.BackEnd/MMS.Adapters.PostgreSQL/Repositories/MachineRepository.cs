using DocumentFormat.OpenXml.Drawing;
using MMS.Application.Common.Functional.Either;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MMS.Adapters.PostgreSQL.Repositories;

public class MachineRepository(
    ApplicationDbContext dbContext,
    ILogger<MachineRepository> logger) : MMsCrudRepository<Machine>(dbContext, logger), IMachineRepository
{
    public async Task DeleteByCustomerIdAsync(Guid customerId)
    {
        var machines = await dbContext.Machines
        .Where(m => m.CustomerId == customerId)
        .ToListAsync();

        dbContext.Machines.RemoveRange(machines);
    }

    public async Task<bool> ExistsAsync(Expression<Func<Machine, bool>> predicate)
    {
        return await dbContext.Machines.AnyAsync(predicate);
    }

    public async Task<bool> ExistsAsyncs(Guid? machineId)
    {
        return await dbContext.Machines.AnyAsync(u => u.Id == machineId);
    }

    public async Task<List<Machine>> GetByIdsAsync(List<string> machineIds)
    {
        if (machineIds == null || !machineIds.Any())
            return new List<Machine>();

        var machines = await dbContext.Machines
            .Where(m => machineIds.Contains(m.Id.ToString()))
            .ToListAsync();

        return machines;
    }

    public async Task<Either<RepositoryError, Machine>> GetBySerialNumberAsync(string serialNumber)
    {
        if (string.IsNullOrEmpty(serialNumber))
        {
            return new RepositoryError("Serial number cannot be null or empty.");
        }

        var query = dbContext.Machines.AsTracking(QueryTrackingBehavior.TrackAll);
        if (Include != null) query = Include(query);
        var entity = await query.FirstOrDefaultAsync(t => t.SerialNumber == serialNumber);

        return entity switch
        {
            null => new EntityNotFound(),
            ISoftDelete softDeleteEntity when softDeleteEntity.IsSoftDeleted() => new EntitySoftDeleted(),
            _ => entity
        };
    }

    public async Task<Either<RepositoryError, Machine>> GetByMachineNameAsync(string machineName)
    {
        if (string.IsNullOrEmpty(machineName))
        {
            return new RepositoryError("Machine name cannot be null or empty.");
        }

        machineName = machineName.Trim().ToLower(); // normalize input

        var query = dbContext.Machines.AsTracking();
        if (Include != null) query = Include(query);

        var entity = await query
            .FirstOrDefaultAsync(t => t.MachineName.Trim().ToLower() == machineName);

        return entity switch
        {
            null => new EntityNotFound(),
            ISoftDelete softDeleteEntity when softDeleteEntity.IsSoftDeleted() => new EntitySoftDeleted(),
            _ => entity
        };
    }

    public async Task<Either<RepositoryError, Machine>> GetAsync(Guid machineId, CancellationToken cancellationToken = default)
    {
        var machine = await dbContext.Machines
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == machineId, cancellationToken);

        if (machine == null)
        {
            return new Left<RepositoryError, Machine>(
                new RepositoryError("Machine not found.")
            );
        }

        return new Right<RepositoryError, Machine>(machine);
    }
}