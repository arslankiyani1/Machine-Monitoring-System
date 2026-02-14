namespace MMS.Application.Ports.In.NoSql.HistoricalStat;


public interface IHistoricalStatsService
{
    Task<ApiResponse<IEnumerable<HistoricalStats>>> GetAllByMachineIdAsync(Guid machineId);
    Task<ApiResponse<IEnumerable<HistoricalStats>>> GetAllAsync(PageParameters pageParameters);
    Task<ApiResponse<HistoricalStats>> GetByIdAsync(string id);
    Task<ApiResponse<HistoricalStats>> CreateAsync(HistoricalStats dto);
    Task<ApiResponse<string>> UpdateAsync(HistoricalStats dto);
    Task<ApiResponse<string>> DeleteAsync(string id);

    Task<ApiResponse<IEnumerable<HistoricalStats>>> CreateHistoricalRecordForDayAsync(DateTime date);
}
