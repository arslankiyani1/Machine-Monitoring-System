namespace MMS.Application.Ports.In.NoSql.MachineStatusSettings;

public interface IMachineStatusSettingService
{
    Task<ApiResponse<IEnumerable<MachineStatusSetting>>> GetAllAsync();
    Task<ApiResponse<MachineStatusSetting>> GetByIdAsync(string id);
    Task<ApiResponse<MachineStatusSetting>> GetByMachineIdAsync(Guid machineId);
    Task<ApiResponse<MachineStatusSetting>> CreateAsync(MachineStatusSetting model);
    Task<ApiResponse<string>> UpdateAsync(string id, MachineStatusSetting model);
    Task<ApiResponse<string>> DeleteAsync(string id);
    Task<ApiResponse<IEnumerable<string>>> GetAllStatusesAsync();
}