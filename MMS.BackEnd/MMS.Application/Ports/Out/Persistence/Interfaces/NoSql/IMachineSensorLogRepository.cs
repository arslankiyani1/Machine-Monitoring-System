namespace MMS.Application.Ports.Out.Persistence.Interfaces.NoSql;

public interface IMachineSensorLogRepository
{
    Task<IEnumerable<MachineSensorLog>> GetAllAsync(List<Guid> machineIds, PageParameters pageParameters);
    Task<MachineSensorLog?> GetByIdAsync(Guid id);
    Task AddAsync(MachineSensorLog entity);
    Task UpdateAsync(MachineSensorLog entity);
    Task DeleteAsync(Guid id);
    Task<IEnumerable<MachineSensorLog>> GetBySensorIdAndTimeRangeAsync(
        Guid sensorId,
        DateTime from,
        DateTime to);
    Task<MachineSensorLog?> GetLatestBySensorIdAsync(Guid sensorId);
    Task<List<MachineSensorLog>> GetLatestBySensorIdsAsync(List<Guid> sensorIds);
}