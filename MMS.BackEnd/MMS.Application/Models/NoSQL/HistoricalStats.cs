namespace MMS.Application.Models.NoSQL;

[Table("HistoricalStats")]
public class HistoricalStats
{
    [Key]
    [JsonPropertyName("id")] public string Id { get; set; } = default!;

    [JsonPropertyName("machine_id")] public Guid MachineId { get; set; }

    [JsonPropertyName("customer_id")] public Guid CustomerId { get; set; }

    [JsonPropertyName("generated_date")] public DateTime GeneratedDate { get; set; }

    [JsonPropertyName("down_time")] public double DownTime { get; set; }

    [JsonPropertyName("historical_downtime_events")]
    public List<HistoricalDowntimeEvent> HistoricalDowntimeEvents { get; set; } = default!;

    [JsonPropertyName("oee")] public double OEE { get; set; }

    [JsonPropertyName("availability")] public double Availability { get; set; }

    [JsonPropertyName("performance")] public double Performance { get; set; }

    [JsonPropertyName("quality")] public double Quality { get; set; }

    [JsonPropertyName("utilization")] public double Utilization { get; set; }

    [JsonPropertyName("qty_completed")] public int QtyCompleted { get; set; }

    [JsonPropertyName("qty_good")] public int QtyGood { get; set; }

    [JsonPropertyName("qty_bad")] public int QtyBad { get; set; }

    [JsonPropertyName("planned_time")] public double PlannedTime { get; set; }

    [JsonPropertyName("required_qty")] public int RequiredQty { get; set; }

    [JsonPropertyName("job_ids")] public List<string> JobIds { get; set; } = new();

    [JsonPropertyName("job_metrics")] public List<JobMetric> JobMetrics { get; set; } = new();
}

public class JobMetric
{
    [JsonPropertyName("id")] public string Id { get; set; } = default!;

    [JsonPropertyName("job_name")] public string JobName { get; set; } = default!;

    [JsonPropertyName("start_time")] public DateTime StartTime { get; set; }

    [JsonPropertyName("end_time")] public DateTime EndTime { get; set; }

    [JsonPropertyName("status")] public JobStatus Status { get; set; } = default!;

    [JsonPropertyName("program_no")] public string ProgramNo { get; set; } = default!;

    [JsonPropertyName("operator_name")] public string OperatorName { get; set; } = default!;

    [JsonPropertyName("oee")] public double OEE { get; set; }

    [JsonPropertyName("availability")] public double Availability { get; set; }

    [JsonPropertyName("performance")] public double Performance { get; set; }

    [JsonPropertyName("quality")] public double Quality { get; set; }

    [JsonPropertyName("qty_completed")] public int QtyCompleted { get; set; }

    [JsonPropertyName("qty_good")] public int QtyGood { get; set; }

    [JsonPropertyName("qty_bad")] public int QtyBad { get; set; }

    [JsonPropertyName("planned_time")] public double PlannedTime { get; set; }

    [JsonPropertyName("required_qty")] public int RequiredQty { get; set; }
}

public class HistoricalDowntimeEvent
{
    [JsonPropertyName("reason")] public string Reason { get; set; } = default!;

    [JsonPropertyName("duration")] public float TotalDuration { get; set; }
}
