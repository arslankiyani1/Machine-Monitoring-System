namespace MMS.Application.Ports.Out.Persistence.Interfaces;


public interface ICustomerReportRepository
{
    Task<List<CustomerReport>> GetListAsync(
        PageParameters pageParameters,
        Expression<Func<CustomerReport, bool>> pageFilterExpression,
        List<Expression<Func<CustomerReport, bool>>>? documentFilterExpression = null,
        Func<IQueryable<CustomerReport>, IOrderedQueryable<CustomerReport>>? order = null
    );

    Task<Either<RepositoryError, CustomerReport>> GetAsync(Guid id);
    Task<Either<RepositoryError, CustomerReport>> AddAsync(CustomerReport entity);
    Task<Either<RepositoryError, CustomerReport>> UpdateAsync(Guid id, CustomerReport entity);
    Task<Either<RepositoryError, CustomerReport>> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}
