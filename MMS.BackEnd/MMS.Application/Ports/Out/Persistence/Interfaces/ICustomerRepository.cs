namespace MMS.Application.Ports.Out.Persistence.Interfaces;

public interface ICustomerRepository
{
    Task<List<Customer>> GetListAsync(
        PageParameters? pageParameters, 
        Expression<Func<Customer, bool>> pageParametersExpression,
        List<Expression<Func<Customer, bool>>>? documentFilterExpression,
        Func<IQueryable<Customer>, IOrderedQueryable<Customer>>? order = null 
    );
    Task<Either<RepositoryError, Customer>> GetAsync(Guid customerId);
    Task<Either<RepositoryError, Customer>> DeleteAsync(Guid customerId);
    Task<Either<RepositoryError, Customer>> AddAsync(Customer customer);
    Task<Either<RepositoryError, Customer>> UpdateAsync(Guid customerId, Customer customer);
    Task<bool> ExistsIgnoreCaseAsync(Expression<Func<Customer, string>> propertySelector, string value);
    Task<bool> ExistsAsync(Guid customerId);
    Task<bool> ExistsEmail(Expression<Func<Customer, bool>> predicate);
    Task<Either<RepositoryError, List<Machine>>> GetMachinesByCustomerIdAsync(Guid customerId);
    Task<Either<RepositoryError, List<Machine>>> GetMachinesByCustomerIdAsync(Guid customerId, PageParameters pageParameters);
    Task<List<CustomerWithMachinesDto>> GetPagedWithMachinesAsync(
       List<Guid>? customerIds,
       string? term,
       int skip,
       int take);
    Task<List<Customer>> GetAllWithMachinesAsync(
        List<Guid>? customerIds = null,
        string? term = null,
        int skip = 0,
        int take = 10);
    Task<bool> ExistsPhoneAsync(string phoneCountryCode, string phoneNumber, Guid? excludeCustomerId = null);
}