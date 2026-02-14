namespace MMS.Application.Ports.In.Machine.Dto;

public class MachineJobSummaryDto
{
    public string Id { get; set; }
    public JobStatus Status { get; set; } = default!;
    public string JobName { get; set; } = default!;
    public string OperatorName { get; set; } = default!;
    public string ProgramNo { get; set; } = default!;
    public int Good { get; set; }
    public int Bad { get; set; }
    public float Oee { get; set; }
    public float Performance { get; set; }
    public float Availability { get; set; }
    public float Quality { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
}
