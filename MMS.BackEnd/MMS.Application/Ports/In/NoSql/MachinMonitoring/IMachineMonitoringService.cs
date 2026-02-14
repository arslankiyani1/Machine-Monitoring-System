using MMS.Application.Ports.In.NoSql.MachineSensorLog.Dto;
using MMS.Application.Ports.In.NoSql.MachinMonitoring.Dto;

namespace MMS.Application.Ports.In.NoSql.MachinMonitoring;

public interface IMachineMonitoringService
{
    Task<ApiResponse<MachineLogSignalRDto>> ProcessMonitoringAsync(MachineMonitoring input);
    Task<ApiResponse<string>> ProcessSensorLogAsync(MachinemonitoringDto dto);
    Task MarkMachineOfflineAsync(Guid machineId);
    Task<ApiResponse<string>> ProcessOperationalDataAsync(CreateOperationalData dto);
}