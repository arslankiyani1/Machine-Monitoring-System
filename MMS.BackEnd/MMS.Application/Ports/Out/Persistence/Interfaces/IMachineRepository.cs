using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MMS.Application.Ports.Out.Persistence.Interfaces;

public interface IMachineRepository
{
    Task<List<Machine>> GetListAsync(
        PageParameters? pageParameters,
        Expression<Func<Machine, bool>> pageParametersExpression,
        List<Expression<Func<Machine, bool>>>? documentFilterExpression,
        Func<IQueryable<Machine>, IOrderedQueryable<Machine>>? order = null
    );
    Task<Either<RepositoryError, Machine>> GetAsync(Guid machineId);
    Task<Either<RepositoryError, Machine>> GetAsync(Guid machineId, CancellationToken cancellationToken = default);
    Task<Either<RepositoryError, Machine>> DeleteAsync(Guid machineId);
    Task<Either<RepositoryError, Machine>> AddAsync(Machine machine);
    Task<Either<RepositoryError, Machine>> UpdateAsync(Guid machineId, Machine machine);
    //TODO : refector this fun 
    Task<bool> ExistsAsyncs(Guid? guid);
    Task<bool> ExistsAsync(Expression<Func<Machine, bool>> predicate);
    Task DeleteByCustomerIdAsync(Guid customerId);
    Task<Either<RepositoryError, Machine>> GetBySerialNumberAsync(string serialNumber);
    Task<Either<RepositoryError, Machine>> GetByMachineNameAsync(string machineName);
    public Task<List<Machine>> GetByIdsAsync(List<string> machineIds);

}