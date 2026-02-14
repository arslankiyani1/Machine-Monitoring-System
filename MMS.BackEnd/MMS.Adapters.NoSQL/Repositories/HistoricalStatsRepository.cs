namespace MMS.Adapters.NoSQL.Repositories;

public class HistoricalStatsRepository(
    MyCosmosDbContext dbContext) : IHistoricalStatsRepository
{
    public async Task AddAsync(HistoricalStats entity)
    {
        await dbContext.HistoricalStats.AddAsync(entity);
        await dbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<HistoricalStats>> GetAllAsync()
    {
        return await dbContext.HistoricalStats
            .OrderByDescending(x => x.GeneratedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<HistoricalStats>> GetAllByMachineIdAsync(Guid machineId)
    {
        return await dbContext.HistoricalStats
            .Where(x => x.MachineId == machineId)
            .OrderByDescending(x => x.GeneratedDate)
            .ToListAsync();
    }

    public async Task<HistoricalStats?> GetByIdAsync(string id)
    {
        return await dbContext.HistoricalStats.FindAsync(id);
    }

    public async Task UpdateAsync(HistoricalStats entity)
    {
        dbContext.HistoricalStats.Update(entity);
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(string id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            dbContext.HistoricalStats.Remove(entity);
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task<List<HistoricalStats>> GetByMachineIdAndDateRangeAsync(Guid machineId, DateTime? start, DateTime? end)
    {
        return await dbContext.HistoricalStats
            .Where(x => x.MachineId == machineId && x.GeneratedDate >= start!.Value && x.GeneratedDate <= end!.Value)
            .OrderBy(x => x.GeneratedDate)
            .ToListAsync();
    }

    public async Task<HistoricalStats?> GetLatestByMachineIdAsync(Guid machineId)
    {
        return await dbContext.HistoricalStats
            .Where(x => x.MachineId == machineId)
            .OrderByDescending(x => x.GeneratedDate)
            .FirstOrDefaultAsync();
    }

    public async Task<HistoricalStats?> GetByMachineAndDateAsync(Guid machineId, DateTime date)
    {
        return await dbContext.HistoricalStats
            .FirstOrDefaultAsync(x => x.MachineId == machineId && x.GeneratedDate == date);
    }

    public async Task<IEnumerable<HistoricalStats>> GetPagedAsync(PageParameters pageParameters)
    {
        var query = dbContext.HistoricalStats.AsQueryable();

        int skip = pageParameters.Skip ?? 0;
        int top = pageParameters.Top ?? 10;

        return await query
            .OrderBy(x => x.Id)
            .Skip(skip)
            .Take(top)
            .ToListAsync();
    }
}
