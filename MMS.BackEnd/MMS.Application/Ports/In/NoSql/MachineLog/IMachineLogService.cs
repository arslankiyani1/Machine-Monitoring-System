namespace MMS.Application.Ports.In.NoSql.MachineLog;

public interface IMachineLogService
{
    Task<ApiResponse<IEnumerable<Models.NoSQL.MachineLog>>> GetAllAsync(PageParameters pageParameters);
    Task<ApiResponse<Models.NoSQL.MachineLog?>> GetByIdAsync(string id);
    Task<ApiResponse<Models.NoSQL.MachineLog>> CreateAsync(Models.NoSQL.MachineLog entity);
    Task<ApiResponse<Models.NoSQL.MachineLog>> UpdateAsync(Models.NoSQL.MachineLog entity);
    Task<ApiResponse<string>> DeleteAsync(string id);

    // Refector Func in Future
    Task<ApiResponse<UtilizationResponseDto>> GetMachineUtilizationAsync(
        Guid machineId, DateTime? from = null, DateTime? to = null);

    Task<ApiResponse<DowntimeApiResponseDto>> GetMachineDowntimeAsync(
                 Guid machineId,
                 Guid? jobId = null,
                 DateTime? from = null,
                 DateTime? to = null);
    Task<ApiResponse<MetricResponseDto>> GetMetricsAsync(Guid machineId, MetricRequestDto request);
    Task<ApiResponse<IEnumerable<ActivitySegment>>> GetMachineTimelineAsync(Guid machineId,
               DateTime? from = null,
               DateTime? to = null);
}