namespace MMS.Application.Ports.In.MachineSensor.Dto;

// ✅ Add
public record AddMachineSensorDto(
    Guid? MachineId,
    Guid? CustomerId,
    string SerialNumber,
    string Name,
    SensorInterface Interface,
    SensorType SensorType,
    string? ModbusIp,
    List<string> HRegList,
    string? ImageUrl
);

// ✅ Update
public record UpdateMachineSensorDto(
    Guid Id,
    Guid? MachineId,
    Guid? CustomerId,
    string SerialNumber,
    string Name,
    SensorInterface Interface,
    SensorType SensorType,
    string? ModbusIp,
    List<string> HRegList,
    string? ImageUrl
);

// ✅ Read (returning to API)
public record MachineSensorDto(
    Guid Id,
    Guid? MachineId,
    Guid? CustomerId,
    string SerialNumber,
    string Name,
    SensorInterface Interface,
    SensorType SensorType,
    string? ModbusIp,
    List<string> HRegList,
    string? ImageUrl,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public class MachineSensorMachinesensorlogDto
{
    public Guid Id { get; set; }
    public Guid? MachineId { get; set; }
    public string Name { get; set; } = default!;
    public string? ImageUrl { get; set; }
    public string? ModbusIp { get; set; }
    public string? Status { get; set; }
    public string SerialNumber { get; set; } = default!;
    public SensorType SensorType { get; set; }
    public SensorInterface Interface { get; set; }
    public DateTime? LastUpdated { get; set; }
}