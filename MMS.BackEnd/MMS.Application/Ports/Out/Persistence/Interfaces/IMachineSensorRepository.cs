
namespace MMS.Application.Ports.Out.Persistence.Interfaces;

public interface IMachineSensorRepository
{
    Task<List<MachineSensor>> GetListAsync(
             PageParameters pageParameters,
             Expression<Func<MachineSensor, bool>> pageFilterExpression,
             List<Expression<Func<MachineSensor, bool>>>? additionalFilters = null,
             Func<IQueryable<MachineSensor>, IOrderedQueryable<MachineSensor>>? order = null
         );

    Task<Either<RepositoryError, MachineSensor>> GetAsync(Guid sensorId);
    Task<Either<RepositoryError, MachineSensor>> DeleteAsync(Guid sensorId);
    Task<Either<RepositoryError, MachineSensor>> AddAsync(MachineSensor sensor);
    Task<Either<RepositoryError, MachineSensor>> UpdateAsync(Guid sensorId, MachineSensor sensor);
    Task<bool> ExistsAsync(Guid? sensorId);
    Task<List<MachineSensor>> GetByCustomerIdAsync(Guid customerId);
    Task<MachineSensor?> GetByNameAsync(string sensorName);
}
