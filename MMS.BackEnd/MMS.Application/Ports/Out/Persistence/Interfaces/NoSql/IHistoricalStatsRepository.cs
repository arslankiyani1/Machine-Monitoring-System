namespace MMS.Application.Ports.Out.Persistence.Interfaces.NoSql;

public interface IHistoricalStatsRepository
{
    Task AddAsync(HistoricalStats entity);
    Task<IEnumerable<HistoricalStats>> GetAllByMachineIdAsync(Guid machineId);
    Task<HistoricalStats?> GetByIdAsync(string id);
    Task UpdateAsync(HistoricalStats entity);
    Task DeleteAsync(string id);
    Task<List<HistoricalStats>> GetByMachineIdAndDateRangeAsync(Guid machineId, DateTime? start, DateTime? end);
    Task<HistoricalStats?> GetLatestByMachineIdAsync(Guid machineId);
    Task<HistoricalStats?> GetByMachineAndDateAsync(Guid machineId, DateTime date);
    Task<IEnumerable<HistoricalStats>> GetPagedAsync(PageParameters pageParameters);
}