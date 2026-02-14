namespace MMS.Application.Ports.Out.Persistence.Interfaces;

public interface ICustomerBillingAddressRepository
{
    Task<List<CustomerBillingAddress>> GetListAsync(
        PageParameters pageParameters,
        Expression<Func<CustomerBillingAddress, bool>> pageFilterExpression,
        List<Expression<Func<CustomerBillingAddress, bool>>>? documentFilterExpression = null,
        Func<IQueryable<CustomerBillingAddress>, IOrderedQueryable<CustomerBillingAddress>>? order = null
    );

    Task<Either<RepositoryError, CustomerBillingAddress>> GetAsync(Guid billingAddressId);
    Task<Either<RepositoryError, CustomerBillingAddress>> AddAsync(CustomerBillingAddress billingAddress);
    Task<Either<RepositoryError, CustomerBillingAddress>> UpdateAsync(Guid billingAddressId, CustomerBillingAddress billingAddress);
    Task<Either<RepositoryError, CustomerBillingAddress>> DeleteAsync(Guid billingAddressId);
}
