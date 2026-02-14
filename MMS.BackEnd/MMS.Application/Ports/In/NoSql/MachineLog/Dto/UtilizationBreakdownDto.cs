namespace MMS.Application.Ports.In.NoSql.MachineLog.Dto;

public class UtilizationBreakdownDto
{
    public Dictionary<string, double> StatusPercent { get; set; } = new();
}
