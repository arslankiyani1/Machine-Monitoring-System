using MMS.Application.Ports.In.NoSql.MachineSensorLog.Dto;

namespace MMS.Application.Ports.In.NoSql.MachinMonitoring.Dto;

public class MachinemonitoringDto
{
    // public Guid SensorId { get; set; }
    public string SensorName { get; set; } = default!;
    public string MachineStatus { get; set; } = default!;
    public List<SensorReadingDto> SensorReading { get; set; } = new();
}