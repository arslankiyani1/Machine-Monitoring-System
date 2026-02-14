
namespace MMS.Application.Ports.Out.Persistence.Interfaces.NoSql;

public interface IMachineStatusSettingRepository
{
    Task<IEnumerable<MachineStatusSetting>> GetAllAsync(IEnumerable<Guid>? machineIds = null);
    Task<MachineStatusSetting?> GetByIdAsync(string id);
    Task<MachineStatusSetting?> GetByIdAsNoTrackingAsync(string id);

    Task AddAsync(MachineStatusSetting entity);
    Task UpdateAsync(MachineStatusSetting entity);
    Task SaveChangesAsync();
    Task DeleteAsync(string id);
    Task<MachineStatusSetting?> GetByMachineIdAsync(Guid machineId);
    Task<IEnumerable<string>> GetAllStatusesAsync();
}