namespace MMS.Adapters.PostgreSQL.Repositories;

public class SubscriptionRepository(ApplicationDbContext dbContext, ILogger<SubscriptionRepository> logger)
    : MMsCrudRepository<Subscription>(dbContext, logger), ISubscriptionRepository
{
    public async Task<bool> ExistsAsync(Guid subscriptionId)
    {
        return await dbContext.CustomerSubscriptions.AnyAsync(x => x.SubscriptionId == subscriptionId);
    }

    public async Task<decimal> GetPriceByIdAsync(Guid subscriptionId)
    {
        var subscription = await dbContext.Subscriptions
        .Where(s => s.Id == subscriptionId)
        .FirstOrDefaultAsync();

        if (subscription == null)
        {
            throw new KeyNotFoundException($"Subscription with ID {subscriptionId} not found.");
        }

        return subscription.Price;
    }
}