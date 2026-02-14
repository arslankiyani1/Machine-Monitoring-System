namespace MMS.Application.Ports.Out.Persistence.Interfaces;

public interface ISubscriptionRepository
{
    Task<List<Subscription>> GetListAsync(
        PageParameters pageParameters,
        Expression<Func<Subscription, bool>> pageFilterExpression,
        List<Expression<Func<Subscription, bool>>>? additionalFilters = null,
        Func<IQueryable<Subscription>, IOrderedQueryable<Subscription>>? order = null
    );
    Task<Either<RepositoryError, Subscription>> GetAsync(Guid subscriptionId);
    Task<Either<RepositoryError, Subscription>> DeleteAsync(Guid subscriptionId);
    Task<Either<RepositoryError, Subscription>> AddAsync(Subscription customerSubscription);
    Task<Either<RepositoryError, Subscription>> UpdateAsync(Guid subscriptionId, Subscription customerSubscription);
    //TODO : refectore func 
    Task<bool> ExistsAsync(Guid subscriptionId);
    Task<decimal> GetPriceByIdAsync(Guid subscriptionId);
}