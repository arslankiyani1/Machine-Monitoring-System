namespace MMS.Adapters.NoSQL.Repositories;

public class CustomerDashboardSummaryRepository(MyCosmosDbContext context) : ICustomerDashboardSummaryRepository
{
    public async Task<IEnumerable<CustomerDashboardSummary>> GetAllAsync(IEnumerable<string>? customerIds = null)
    {
        var query = context.CustomerDashboardSummaries.AsQueryable();
        if (customerIds != null && customerIds.Any())
        {
            query = query.Where(x => customerIds.Contains(x.CustomerId));
        }
        return await query.ToListAsync();
    }

    public async Task<CustomerDashboardSummary?> GetByCustomerIdAsync(string customerId)
    {
        try
        {
            return await context.CustomerDashboardSummaries
                .WithPartitionKey(customerId)
                .FirstOrDefaultAsync(x => x.Id == customerId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to get CustomerDashboardSummary for CustomerId={customerId}: {ex.Message}");
            return null;
        }
    }

    public async Task UpsertAsync(CustomerDashboardSummary entity)
    {
        if (string.IsNullOrEmpty(entity.CustomerId))
        {
            throw new ArgumentException("CustomerId is required.");
        }

        entity.Id = entity.CustomerId; // Ensure Id matches CustomerId for consistency

        var existing = await GetByCustomerIdAsync(entity.CustomerId);

        try
        {
            if (existing == null)
            {
                entity.CreatedAt = DateTime.UtcNow;
                entity.UpdatedAt = DateTime.UtcNow;
                context.CustomerDashboardSummaries.Add(entity);
            }
            else
            {
                existing.StatusSummary = entity.StatusSummary;
                existing.UpdatedAt = DateTime.UtcNow;
                context.CustomerDashboardSummaries.Update(existing);
            }
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error saving to Cosmos DB: " + ex.Message, ex);
        }
    }

    public async Task DeleteAsync(string customerId)
    {
        var entity = await GetByCustomerIdAsync(customerId);
        if (entity != null)
        {
            context.CustomerDashboardSummaries.Remove(entity);
            await context.SaveChangesAsync();
        }
    }
}
