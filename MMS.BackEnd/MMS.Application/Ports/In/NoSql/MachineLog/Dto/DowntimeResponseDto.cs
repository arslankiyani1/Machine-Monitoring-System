namespace MMS.Application.Ports.In.NoSql.MachineLog.Dto;


public class DowntimeResponseDto
{
    public string Reason { get; set; } = default!;
    public string Color { get; set; } = default!;
    public float Duration { get; set; }
    public float Percentage { get; set; }
    public string JobId { get; set; } = default!;
}

public class DowntimeApiResponseDto
{
    public double TotalDowntime { get; set; }
    public double TotalDowntimePercent { get; set; } // %
    public List<DowntimeResponseDto> DowntimeMetrics { get; set; } = new();
}


public class UtilizationResponseDto
{
    public double TotalUtilization { get; set; }
    public Dictionary<string, double> StatusPercent { get; set; } = new();
    public Dictionary<string, string> StatusColors { get; set; } = new();

}

public class JobMetricsFromLogs
{
    public double TotalDowntimeSeconds { get; set; }
    public float TotalDowntimePercent { get; set; }  // ✅ Added
    public double TotalRunningSeconds { get; set; }
    public double PlannedSeconds { get; set; }
    public double OperatingSeconds { get; set; }
    public float Utilization { get; set; }
    public float Availability { get; set; }
    public float Performance { get; set; }
    public float Quality { get; set; }
    public float Oee { get; set; }
    public List<DowntimeResponseDto> DowntimeBreakdown { get; set; } = new();

    public static JobMetricsFromLogs Empty() => new();
}