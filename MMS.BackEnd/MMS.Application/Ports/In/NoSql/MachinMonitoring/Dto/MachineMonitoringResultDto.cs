namespace MMS.Application.Ports.In.NoSql.MachinMonitoring.Dto;

public class MachineMonitoringResultDto
{
    public bool Matched { get; set; }
    public string? Status { get; set; }
    public string? Color { get; set; }
    public string Message { get; set; } = default!;


}
