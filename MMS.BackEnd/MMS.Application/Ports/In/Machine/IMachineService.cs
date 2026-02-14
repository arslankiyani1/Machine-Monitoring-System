namespace MMS.Application.Ports.In.Machine;

public interface IMachineService
{
    Task<ApiResponse<IEnumerable<MachineDto>>> GetListAsync(PageParameters pageParameters,
        CommunicationProtocol? protocol, MachineType? type,Guid CustomerId);
    Task<ApiResponse<MachineDto>> GetByIdAsync(Guid machineId, CancellationToken cancellationToken = default);
    Task<ApiResponse<MachineDto>> GetBySerialNumber(string serialNumber, CancellationToken cancellationToken = default);

    Task<ApiResponse<MachineDto>> GetByMachineName(string machineName, CancellationToken cancellationToken = default);


    Task<ApiResponse<IEnumerable<MachineJobSummaryDto>>> GetSummaryByIdAsync(Guid id,
    TimeRange range,
    DateTime? from = null,
    DateTime? to = null);
    Task<ApiResponse<MachineDto>> AddAsync(AddMachineDto request);
    Task<ApiResponse<MachineDto>> UpdateAsync(Guid machineId, UpdateMachineDto request);
    Task<ApiResponse<string>> DeleteAsync(Guid machineId);
    Task<ApiResponse<MachineJobDto>> GetMachineDetailsByIdAsync(Guid id);
    Task<ApiResponse<string>> CreateDefaultSettingsForMachineAsync(Guid machineId);
}