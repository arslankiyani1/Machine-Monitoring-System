namespace MMS.Adapters.NoSQL.Repositories;

public class MachineSensorDataRepository(MyCosmosDbContext dbContext) : IMachineSensorLogRepository
{
    public async Task AddAsync(MachineSensorLog entity)
    {
        await dbContext.MachineSensorData.AddAsync(entity);
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            dbContext.MachineSensorData.Remove(entity);
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<MachineSensorLog>> GetAllAsync(List<Guid> machineIds, PageParameters pageParameters)
    {
        var query = dbContext.MachineSensorData
         .Where(d => machineIds.Contains(d.MachineId!))
         .AsQueryable();

        int skip = pageParameters.Skip ?? 0;
        int top = pageParameters.Top ?? 10;

        return await query
            .OrderByDescending(d => d.DateTime)
            .Skip(skip)
            .Take(top)
            .ToListAsync();
    }

    public async Task<MachineSensorLog?> GetByIdAsync(Guid id)
        => await dbContext.MachineSensorData.FindAsync(id);

    public async Task<IEnumerable<MachineSensorLog>> GetBySensorIdAndTimeRangeAsync(
    Guid sensorId,
    DateTime from,
    DateTime to)
    {
        return await dbContext.MachineSensorData
            .Where(x => x.SensorId == sensorId && x.DateTime >= from && x.DateTime <= to)
            .OrderByDescending(x => x.DateTime)
            .ToListAsync();
    }

    public async Task<MachineSensorLog?> GetLatestBySensorIdAsync(Guid sensorId)
    {
        return await dbContext.MachineSensorData
        .Where(x => x.SensorId == sensorId)
        .OrderByDescending(x => x.DateTime)
        .FirstOrDefaultAsync();
    }

    public async Task<List<MachineSensorLog>> GetLatestBySensorIdsAsync(List<Guid> sensorIds)
    {
        if (!sensorIds.Any())
            return new List<MachineSensorLog>();

        var allLogs = await dbContext.MachineSensorData
            .AsNoTracking()
            .Where(l => sensorIds.Contains(l.SensorId))
            .OrderByDescending(l => l.DateTime)
            .ToListAsync();

        return allLogs
            .GroupBy(l => l.SensorId)
            .Select(g => g.First())
            .ToList();
    }

    public async Task UpdateAsync(MachineSensorLog entity)
    {
        var existing = await dbContext.MachineSensorData
        .FirstOrDefaultAsync(x => x.Id == entity.Id && x.MachineId == entity.MachineId);

        if (existing == null)
            throw new KeyNotFoundException($"MachineSensorData with Id {entity.Id} not found.");

        dbContext.Entry(existing).CurrentValues.SetValues(entity);

        await dbContext.SaveChangesAsync();
    }
}
