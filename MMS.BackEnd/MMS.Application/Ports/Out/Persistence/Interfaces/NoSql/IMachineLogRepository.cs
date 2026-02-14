namespace MMS.Application.Ports.Out.Persistence.Interfaces.NoSql;

public interface IMachineLogRepository
{
    Task<MachineLog?> GetLastOpenLogByMachineId(Guid machineId);
    Task AddAsync(MachineLog entity);
    Task AddMachineLogAsync(MachineLog entity);
    Task SaveChangesAsync();
    Task UpdateMachineLogAsync(MachineLog entity);
    Task<MachineLog?> GetByIdAsync(string id);
    Task UpdateAsync(MachineLog entity);
    Task DeleteAsync(string id);
    Task<MachineLog?> GetLastestLogMachineIdAsync(Guid machineId);
    Task<List<MachineLog>> GetByMachineIdsLatestLogAsync(List<Guid> machineIds);
    Task<List<MachineLog>> GetLogsByMachineIdAndDateRangeAsync(Guid machineId, DateTime start, DateTime end);
    Task<IEnumerable<MachineLog>> GetByMachineIdAsync(Guid machineId);
    // spindle speed interface 
    Task<List<MachineLog>> GetMachineLogsAsync(Guid machineId, DateTime? from, DateTime? to, bool includeEndNull);
    Task<List<MachineLog>> GetByMachineIdsForCustomerDashboardAsync(List<Guid> machineIds, DateTime start, DateTime end);
    Task<IEnumerable<MachineLog>> GetPagedAsync(IEnumerable<Guid> machineIds, PageParameters pageParameters);
    Task<List<MachineLog>> GetDowntimeLogsByMachineIdAsync(Guid machineId, DateTime s, DateTime e, Guid? jobId);
    Task<MachineLog?> GetLatestLogByMachineIdAsync(Guid machineId);
    Task<List<MachineLog>> GetAllOpenLogsByMachineIdForUpdateAsync(Guid id);
    Task CloseMultipleLogsAsync(List<MachineLog> logs, DateTime closeTime, string? source = null);
    Task<List<MachineLog>> GetDistinctDowntimeColorsAsync();
    Task<List<MachineLog>> GetByJobIdAsync(string jobId);
    Task<List<MachineLog>> GetByMachineIdsAndDateRangeAsync(
            List<Guid> machineIds,
            DateTime start,
            DateTime end);
}
