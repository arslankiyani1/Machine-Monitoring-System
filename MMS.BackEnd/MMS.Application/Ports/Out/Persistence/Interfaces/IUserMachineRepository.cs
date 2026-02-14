namespace MMS.Application.Ports.Out.Persistence.Interfaces;

public interface IUserMachineRepository
{
    Task<List<UserMachine>> GetListAsync(
        PageParameters? pageParameters,
        Expression<Func<UserMachine, bool>> pageFilterExpression,
        List<Expression<Func<UserMachine, bool>>>? documentFilterExpression = null,
        Func<IQueryable<UserMachine>, IOrderedQueryable<UserMachine>>? order = null
    );
    Task<Either<RepositoryError, UserMachine>> GetAsync(Guid userMachineId);
    Task<Either<RepositoryError, UserMachine>> AddAsync(UserMachine userMachine);
    Task<Either<RepositoryError, UserMachine>> UpdateAsync(Guid userMachineId, UserMachine userMachine);
    Task<Either<RepositoryError, UserMachine>> DeleteAsync(Guid userMachineId);
    Task<bool> ExistsAsync(Expression<Func<UserMachine, bool>> predicate);
    Task DeleteRangeAsync(List<UserMachine> userMachines);


}