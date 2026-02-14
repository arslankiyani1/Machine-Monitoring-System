namespace MMS.Application.Ports.In.MachineSensor;

public interface IMachineSensorService
{
    Task<ApiResponse<IEnumerable<MachineSensorDto>>> GetListAsync(PageParameters pageParameters);
    Task<ApiResponse<MachineSensorDto>> GetByIdAsync(Guid sensorId);
    Task<ApiResponse<MachineSensorDto>> AddAsync(AddMachineSensorDto request);
    Task<ApiResponse<MachineSensorDto>> UpdateAsync(UpdateMachineSensorDto request);
    Task<ApiResponse<string>> DeleteAsync(Guid sensorId);
    Task<ApiResponse<IEnumerable<MachineSensorMachinesensorlogDto>>> GetSensorsByCustomerIdAsync(Guid customerId,
        PageParameters pageParameters);
}
