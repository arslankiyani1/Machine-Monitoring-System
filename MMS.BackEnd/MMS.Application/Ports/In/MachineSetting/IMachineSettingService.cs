namespace MMS.Application.Ports.In.MachineSetting;

public interface IMachineSettingService
{
    Task<ApiResponse<IEnumerable<MachineSettingDto>>> GetListAsync(PageParameters pageParameters,
        MachineSettingsStatus? status, Guid? MachineId);
    Task<ApiResponse<MachineSettingDto>> GetByIdAsync(Guid machineSettingId);
    Task<ApiResponse<MachineSettingDto>> AddAsync(AddMachineSettingDto request);
    Task<ApiResponse<MachineSettingDto>> UpdateAsync(Guid machineSettingId, UpdateMachineSettingDto request);
    Task<ApiResponse<string>> DeleteAsync(Guid machineSettingId);
}