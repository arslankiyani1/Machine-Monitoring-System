namespace MMS.Application.Models.SQL;

public class MachineSensor : Trackable
{
    public Guid? MachineId { get; set; }
    public string SerialNumber { get; set; } = default!;
    public Guid? CustomerId { get; set; } = default!;
    public string Name { get; set; } = default!;
    public SensorInterface Interface { get; set; }
    public SensorType SensorType { get; set; }
    public string? ModbusIp { get; set; }
    public ICollection<string> HRegList { get; set; } = [];
    public string? ImageUrl { get; set; } = default!;
}