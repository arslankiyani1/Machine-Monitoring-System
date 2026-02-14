namespace MMS.Adapters.NoSQL.Repositories;

public class MachineStatusSettingRepository(MyCosmosDbContext context)
    : IMachineStatusSettingRepository
{
    public async Task<IEnumerable<MachineStatusSetting>> GetAllAsync(IEnumerable<Guid>? machineIds = null)
    {
        var query = context.MachineStatusSettings.AsQueryable();
        if (machineIds != null && machineIds.Any())
        {
            query = query.Where(x => machineIds.Contains(x.MachineId));
        }
        return await query.ToListAsync();
    }

    public async Task<MachineStatusSetting?> GetByIdAsync(string id)
        => await context.MachineStatusSettings.FindAsync(id);

    public async Task<MachineStatusSetting?> GetByIdAsNoTrackingAsync(string id) =>
         await context.MachineStatusSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id); 

    public async Task AddAsync(MachineStatusSetting entity)
    {
        try
        {
            context.MachineStatusSettings.Add(entity);
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error saving to Cosmos DB: " + ex.Message, ex);
        }
    }

    public async Task UpdateAsync(MachineStatusSetting entity)
    {
        context.MachineStatusSettings.Update(entity);
        await context.SaveChangesAsync();
    }

    public async Task SaveChangesAsync()
    {
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(string id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            context.MachineStatusSettings.Remove(entity);
            await context.SaveChangesAsync();
        }
    }

    public async Task<MachineStatusSetting?> GetByMachineIdAsync(Guid machineId)
    {
        try
        {
            return await context.MachineStatusSettings
                .FirstOrDefaultAsync(x => x.MachineId == machineId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to get MachineStatusSetting for MachineId={machineId}: {ex.Message}");
            return null;
        }
    }

    public async Task<IEnumerable<string>> GetAllStatusesAsync()
    {
        try
        {
            // Load all MachineStatusSettings into memory first
            var allSettings = await context.MachineStatusSettings
                .AsNoTracking()
                .ToListAsync();

            // Now safely flatten the nested Inputs list in memory
            var statuses = allSettings
                .Where(x => x.Inputs != null)
                .SelectMany(x => x.Inputs!)
                .Select(i => i.Status)
                .Where(s => !string.IsNullOrEmpty(s))
                .Distinct()
                .ToList();

            return statuses;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to get all statuses: {ex.Message}");
            throw new InvalidOperationException("Error fetching statuses from Cosmos DB", ex);
        }
    }
}