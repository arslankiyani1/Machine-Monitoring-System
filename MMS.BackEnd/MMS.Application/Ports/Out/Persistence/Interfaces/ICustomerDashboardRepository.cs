namespace MMS.Application.Ports.Out.Persistence.Interfaces;

public interface ICustomerDashboardRepository
{
    Task<List<CustomerDashboard>> GetListAsync(
        PageParameters? pageParameters,
        Expression<Func<CustomerDashboard, bool>> pageParametersExpression,
        List<Expression<Func<CustomerDashboard, bool>>>? documentFilterExpression,
        Func<IQueryable<CustomerDashboard>, IOrderedQueryable<CustomerDashboard>>? order = null
    );

    Task<Either<RepositoryError, CustomerDashboard>> GetAsync(Guid dashboardId);
    Task<Either<RepositoryError, CustomerDashboard>> DeleteAsync(Guid dashboardId);
    Task<Either<RepositoryError, CustomerDashboard>> AddAsync(CustomerDashboard dashboard);
    Task<Either<RepositoryError, CustomerDashboard>> UpdateAsync(Guid dashboardId, CustomerDashboard dashboard);
    //TODO refector this fun
    Task<bool> ExistsByCustomerIdAsync(Guid? customerId);
    Task<bool> ExistsAsync(Guid? dashboardId);
    Task DeleteByCustomerIdAsync(Guid customerId);
}