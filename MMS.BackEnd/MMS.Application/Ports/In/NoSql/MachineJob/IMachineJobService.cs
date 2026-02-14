

using MMS.Application.Ports.In.NoSql.MachineJob.Dto;

namespace MMS.Application.Ports.In.NoSql.MachineJob;

public interface IMachineJobService
{
    Task<ApiResponse<IEnumerable<MMS.Application.Models.NoSQL.MachineJob>>> GetAllAsync(PageParameters pageParameters, 
        Guid? machineId, Guid? customerId);
    Task<ApiResponse<MMS.Application.Models.NoSQL.MachineJob>> GetByIdAsync(string id);
    Task<ApiResponse<Dto.MachineJobDto>> CreateAsync(MachineJobAddDto entity);
    Task<ApiResponse<Dto.MachineJobDto>> UpdateAsync(MachineJobUpdateDto entity);
    Task<ApiResponse<string>> DeleteAsync(string id);
    Task<ApiResponse<JobStatusSummaryDto>> GetJobSummaryAsync(PageParameters pageParameters,Guid customerId);
    Task<ApiResponse<JobDetailsStats>> GetJobDetailsStatsAsync(string jobId);
}