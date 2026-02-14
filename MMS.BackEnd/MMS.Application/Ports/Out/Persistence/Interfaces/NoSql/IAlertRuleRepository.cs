
using System.Threading;

namespace MMS.Application.Ports.Out.Persistence.Interfaces.NoSql;

public interface IAlertRuleRepository
{
    Task<IEnumerable<AlertRule>> GetAllAsync();
    Task<AlertRule?> GetByIdAsync(string id);
    Task<AlertRule> AddAsync(AlertRule entity);
    Task<AlertRule?> UpdateAsync(AlertRule entity, CancellationToken cancellationToken = default);
    public Task<bool> DeleteAsync(string id);
    Task<IEnumerable<AlertRule>> GetPagedAsync(PageParameters pageParameters, Guid? machineId);
    Task<IEnumerable<AlertRule>> GetEnabledByMachineIdAsync(Guid machineId, CancellationToken cancellationToken = default);
    Task<IEnumerable<AlertRule>> GetAlertScopeByMachineIdAsync(Guid machineId, CancellationToken cancellationToken = default);

}
