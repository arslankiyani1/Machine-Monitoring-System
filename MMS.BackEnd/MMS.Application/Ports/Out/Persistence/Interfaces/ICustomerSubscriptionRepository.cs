namespace MMS.Application.Ports.Out.Persistence.Interfaces;

public interface ICustomerSubscriptionRepository
{
    Task<List<CustomerSubscription>> GetListAsync(
        PageParameters pageParameters,
        Expression<Func<CustomerSubscription, bool>> pageFilterExpression,
        List<Expression<Func<CustomerSubscription, bool>>>? documentFilterExpression = null,
        Func<IQueryable<CustomerSubscription>, IOrderedQueryable<CustomerSubscription>>? order = null
    );
    Task<Either<RepositoryError, CustomerSubscription>> GetAsync(Guid subscriptionId);
    Task<Either<RepositoryError, CustomerSubscription>> DeleteAsync(Guid subscriptionId);
    Task<Either<RepositoryError, CustomerSubscription>> AddAsync(CustomerSubscription customerSubscription);
    Task<Either<RepositoryError, CustomerSubscription>> UpdateAsync(CustomerSubscription customerSubscription);
    //TODO refector this fun
    Task<bool> ExistsByCustomerIdAsync(Guid customerId);
    Task<bool> AnyAsync(Expression<Func<CustomerSubscription, bool>> predicate);
    Task<IEnumerable<CustomerSubscription>> GetByCustomerIdAsync(Guid customerId);
    Task<Either<RepositoryError, CustomerSubscription>> ExistingActiveSubAsync(Guid customerId);

}
