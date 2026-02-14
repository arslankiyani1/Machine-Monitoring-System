using MMS.Application.Ports.In.NoSql.MachineSensorLog.Dto;

namespace MMS.Application.Ports.In.NoSql.MachineSensorData;

public interface IMachineSensorLogService
{
    Task<ApiResponse<IEnumerable<MachineSensorLogDto>>> GetAllAsync(PageParameters pageParameters);
    Task<ApiResponse<MachineSensorLogDto>> GetByIdAsync(Guid id);
    Task<ApiResponse<MachineSensorLogDto>> CreateAsync(AddMachineSensorLogDto dto);
    Task<ApiResponse<MachineSensorLogDto>> UpdateAsync(UpdateMachineSensorLogDto dto);
    Task<ApiResponse<Guid>> DeleteAsync(Guid id);

    Task<ApiResponse<IEnumerable<MachineSensorLogsDto>>> GetLatestSensorDataAsync(Guid sensorId,
        DateTime? from,
        DateTime? to);
    Task <ApiResponse<IEnumerable<SensorTrendDataDto>>> GetSensorTrendAsync( Guid sensorId, DateTime? from,
        DateTime? to); 
}
