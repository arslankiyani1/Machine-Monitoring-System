using System.Globalization;

namespace MMS.Application.Ports.In.Machine.Dto;

public class MachineJobDto
{
    public Guid MachineId { get; set; }
    public string MachineModel { get; set; } = default!;
    public Guid UserId { get; set; }
    public string UserName { get; set; } = default!;
    public string MachineName { get; set; } = default!;
    public string? SerialNumber { get; set; }
    public string OperatorImageUrl { get; set; } = default!;
    public string? ImageUrl { get; set; } = default!;

    // Metrics from MongoDB
    public float Oee { get; set; }
    public float Performance { get; set; }
    public float Availability { get; set; }
    public float Quality { get; set; }
    public string? lastLogStatus { get; set; } = default!;
    public string? lastLogColor { get; set; } = default!;
    public DateTime? lastUpdatedTime { get; set; }
    public List<ActivitySegment> Activity { get; set; } = new();
}

public class ActivitySegment
{
    public string Status { get; set; } = default!;
    public DateTime Start { get; set; }
    public DateTime? End { get; set; }
    public string Color { get; set; } = default!;
    public string JobId { get; set; } = default!;
    public string JobName { get; set; } = default!;
    public string UserName { get; set; } = default!;
    public string MainProgram { get; set; } = default!;
    public string CurrentProgram { get; set; } = default!;
    public bool IsRunning { get; set; }

    public double DurationHoursRaw =>
    IsRunning
        ? (DateTime.UtcNow - Start).TotalHours
        : (End.HasValue ? (End.Value - Start).TotalHours : 0);

    public string DurationHours =>
        DurationHoursRaw.ToString("0.####################", CultureInfo.InvariantCulture);
}