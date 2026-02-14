namespace MMS.Adapters.PostgreSQL.Repositories;

public class CustomerSubscriptionRepository
    (ApplicationDbContext dbContext,
    ILogger<CustomerSubscriptionRepository> logger) : MMsCrudRepository<CustomerSubscription>(dbContext, logger), ICustomerSubscriptionRepository
{
    public async Task<bool> AnyAsync(Expression<Func<CustomerSubscription, bool>> predicate)
    {
        return await dbContext.CustomerSubscriptions.AnyAsync(predicate);
    }

    public async Task<Either<RepositoryError, CustomerSubscription>> ExistingActiveSubAsync(Guid customerId)
    {
        var subscription = await dbContext.CustomerSubscriptions
       .FirstOrDefaultAsync(s => s.CustomerId == customerId && s.IsActive);

        if (subscription == null)
            return Either<RepositoryError, CustomerSubscription>.FromLeft(
                new RepositoryError("No active subscription found"));

        return subscription;
    }

    public async Task<bool> ExistsByCustomerIdAsync(Guid customerId)
    {
        return await dbContext.CustomerSubscriptions
                  .AnyAsync(cs => cs.CustomerId == customerId);
    }

    public async Task<IEnumerable<CustomerSubscription>> GetByCustomerIdAsync(Guid customerId)
    {
        return await dbContext.CustomerSubscriptions
       .Where(s => s.CustomerId == customerId)
       .ToListAsync();
    }

    public async Task<Either<RepositoryError, CustomerSubscription>> UpdateAsync(CustomerSubscription customerSubscription)
    {
        try
        {
            dbContext.CustomerSubscriptions.Update(customerSubscription);
            await dbContext.SaveChangesAsync();
            return customerSubscription; 
        }
        catch (Exception ex)
        {
            return Either<RepositoryError, CustomerSubscription>.FromLeft(
                new RepositoryError("An unexpected error occurred while updating the subscription.", ex)
            );
        }
    }
}