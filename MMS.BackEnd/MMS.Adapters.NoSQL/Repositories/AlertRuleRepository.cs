using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using MMS.Application.Common.Dto;

namespace MMS.Adapters.NoSQL.Repositories;

public class AlertRuleRepository(MyCosmosDbContext dbContext) : IAlertRuleRepository
{
    async Task<AlertRule> IAlertRuleRepository.AddAsync(AlertRule entity)
    {
        await dbContext.AlertRule.AddAsync(entity);
        await dbContext.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            dbContext.AlertRule.Remove(entity);
            await dbContext.SaveChangesAsync();
        }
        return entity != null;
    }

    public async Task<IEnumerable<AlertRule>> GetAllAsync()
    {
        return await dbContext.AlertRule.ToListAsync();
    }

    public async Task<AlertRule?> GetByIdAsync(string id)
    {
        return await dbContext.AlertRule
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    // ✅ FIXED: Use Entry().CurrentValues.SetValues() for CosmosDB
    public async Task<AlertRule?> UpdateAsync(AlertRule entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
        {
            Console.WriteLine($"❌ Error: Received null AlertRule in UpdateAsync");
            return null;
        }

        try
        {
            var trackedEntity = await dbContext.AlertRule
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.CustomerId == entity.CustomerId, cancellationToken);

            if (trackedEntity == null)
            {
                Console.WriteLine($"❌ Error: AlertRule not found for Id: {entity.Id}");
                return null;
            }

            // ✅ SOLUTION: Detach the tracked entity and attach the updated one
            dbContext.Entry(trackedEntity).State = EntityState.Detached;

            // ✅ Set the updated timestamp
            entity.UpdatedAt = DateTime.UtcNow;
            entity.CreatedAt = trackedEntity.CreatedAt; // Preserve original CreatedAt

            // ✅ Attach and mark as modified - this forces CosmosDB to replace the entire document
            dbContext.AlertRule.Update(entity);

            // Save changes
            await dbContext.SaveChangesAsync(cancellationToken);
            return entity;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            Console.WriteLine($"❌ Concurrency error updating AlertRule Id: {entity.Id}. Exception: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: Failed to update AlertRule Id: {entity.Id}. Exception: {ex.Message}");
            return null;
        }
    }

    public async Task<IEnumerable<AlertRule>> GetPagedAsync(PageParameters pageParameters, Guid? machineId)
    {
        int skip = pageParameters.Skip ?? 0;
        int top = pageParameters.Top ?? 10;

        try
        {
            var query = dbContext.AlertRule.AsQueryable();

            if (!string.IsNullOrWhiteSpace(pageParameters.Term))
            {
                query = query.Where(rule => rule.RuleName.Contains(pageParameters.Term, StringComparison.OrdinalIgnoreCase));
            }

            if (machineId.HasValue && machineId.Value != Guid.Empty)
            {
                query = query.Where(r => r.MachineId == machineId.Value);
            }

            return await query
                .Skip(skip)
                .Take(top)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred while retrieving paged alert rules.", ex);
        }
    }

    public async Task<IEnumerable<AlertRule>> GetEnabledByMachineIdAsync(Guid machineId, CancellationToken cancellationToken = default)
    {
        if (machineId == Guid.Empty)
        {
            return new List<AlertRule>();
        }

        try
        {
            var rules = await dbContext.AlertRule
                .Where(r => r.MachineId == machineId && r.Enabled)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return rules;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: Failed to fetch alert rules for MachineId: {machineId}. Exception: {ex.Message}");
            return new List<AlertRule>();
        }
    }

    public async Task<IEnumerable<AlertRule>> GetAlertScopeByMachineIdAsync(Guid machineId, CancellationToken cancellationToken = default)
    {
        if (machineId == Guid.Empty)
        {
            return new List<AlertRule>();
        }

        try
        {
            var rules = await dbContext.AlertRule
                .Where(r => r.MachineId == machineId
                    && r.Enabled
                    && (r.AlertScope == AlertScope.Sensor || r.AlertScope == AlertScope.Machine))
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return rules;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: Failed to fetch alert rules for MachineId: {machineId}. Exception: {ex.Message}");
            return new List<AlertRule>();
        }
    }
}