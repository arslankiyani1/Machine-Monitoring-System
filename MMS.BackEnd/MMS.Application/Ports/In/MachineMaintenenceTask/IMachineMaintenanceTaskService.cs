namespace MMS.Application.Ports.In.MachineMaintenenceTask;

public interface IMachineMaintenanceTaskService
{
    Task<ApiResponse<IEnumerable<MachineMaintenanceTaskDto>>> GetListAsync(
   PageParameters pageParameters, Guid? machineId);
    Task<ApiResponse<MachineMaintenanceTaskDto>> GetByIdAsync(Guid taskId);
    Task<ApiResponse<MachineMaintenanceTaskDto>> AddAsync(AddMachineMaintenanceTaskDto request);
    Task<ApiResponse<MachineMaintenanceTaskDto>> UpdateAsync(Guid taskId, UpdateMachineMaintenanceTaskDto request);
    Task<ApiResponse<string>> DeleteAsync(Guid taskId);
}
