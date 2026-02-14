namespace MMS.Application.Ports.Out.Persistence.Interfaces;

public interface IMachineSettingRepository
{
    Task<List<MachineSetting>> GetListAsync(
            PageParameters pageParameters,
            Expression<Func<MachineSetting, bool>> pageFilterExpression,
            List<Expression<Func<MachineSetting, bool>>>? documentFilterExpression = null,
            Func<IQueryable<MachineSetting>, IOrderedQueryable<MachineSetting>>? order = null
        );
    Task<Either<RepositoryError, MachineSetting>> GetAsync(Guid machineSettingId);
    Task<Either<RepositoryError, MachineSetting>> DeleteAsync(Guid machineSettingId);
    Task<Either<RepositoryError, MachineSetting>> AddAsync(MachineSetting machineSetting);
    Task<Either<RepositoryError, MachineSetting>> UpdateAsync(Guid machineSettingId, MachineSetting machineSetting);
    Task<bool> ExistsByMachineIdAsync(Guid machineId);
    Task<MachineSetting?> GetByMachineIdAsync(Guid machineId);
}
