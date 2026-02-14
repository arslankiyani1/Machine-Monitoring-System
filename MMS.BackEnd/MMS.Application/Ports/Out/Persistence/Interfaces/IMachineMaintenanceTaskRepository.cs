namespace MMS.Application.Ports.Out.Persistence.Interfaces;

public interface IMachineMaintenanceTaskRepository
{
    Task<List<MachineMaintenanceTask>> GetListAsync(
            PageParameters pageParameters,
            Expression<Func<MachineMaintenanceTask, bool>> pageFilterExpression,
            List<Expression<Func<MachineMaintenanceTask, bool>>>? documentFilterExpression = null,
            Func<IQueryable<MachineMaintenanceTask>, IOrderedQueryable<MachineMaintenanceTask>>? order = null
        );

    Task<Either<RepositoryError, MachineMaintenanceTask>> GetAsync(Guid taskId);
    Task<Either<RepositoryError, MachineMaintenanceTask>> DeleteAsync(Guid taskId);
    Task<Either<RepositoryError, MachineMaintenanceTask>> AddAsync(MachineMaintenanceTask task);
    Task<Either<RepositoryError, MachineMaintenanceTask>> UpdateAsync(Guid taskId, MachineMaintenanceTask task);
    Task<bool> ExistsAsync(Guid taskId);
    Task DeleteByCustomerIdAsync(Guid machieId);
    Task DeleteByMachineIdAsync(Guid machineId);
}
