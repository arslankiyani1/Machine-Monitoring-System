namespace MMS.Application.Ports.In.Customer.Dto;

public class MachineStatusSummaryDto
{
    public int TotalMachines { get; set; }
    public int online { get; set; }
    public int offline { get; set; }
    public int warning { get; set; }
    public int inProduction { get; set; }
}

public class CustomerWithMachineStatsDto
{
    public string Name { get; set; } = default!;
    public string? ImageUrl { get; set; }
    public MachineStatusSummaryDto MachineSummary { get; set; } = default!;
}
