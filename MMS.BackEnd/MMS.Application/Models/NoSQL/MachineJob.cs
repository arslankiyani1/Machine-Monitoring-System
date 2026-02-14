namespace MMS.Application.Models.NoSQL;

[Table("MachineJob")]
public class MachineJob
{
    [Key]
    [JsonPropertyName("id")] public string Id { get; set; } = default!;
    [JsonPropertyName("job_name")] public string JobName { get; set; } = default!;
    [JsonPropertyName("customer_id")] public Guid CustomerId { get; set; }
    [JsonPropertyName("machine_ids")] public List<string> MachineIds { get; set; } = default!;
    [JsonPropertyName("machine_names")] public List<string> MachineNames { get; set; } = default!;
    [JsonPropertyName("operator_id")] public Guid OperatorId { get; set; }
    [JsonPropertyName("operator_name")] public string OperatorName { get; set; } = default!;
    [JsonPropertyName("status")] public JobStatus Status { get; set; } = default!;
    [JsonPropertyName("start_time")] public DateTime StartTime { get; set; }
    [JsonPropertyName("end_time")] public DateTime EndTime { get; set; }
    [JsonPropertyName("part_number")] public string PartNumber { get; set; } = default!; 
    [JsonPropertyName("program_no")] public string ProgramNo { get; set; } = default!;
    [JsonPropertyName("main_program")] public string MainProgram { get; set; } = default!;
    [JsonPropertyName("description")] public string Description { get; set; } = default!;
    [JsonPropertyName("order_no")] public string OrderNo { get; set; } = default!;
    [JsonPropertyName("order_date")] public DateTime OrderDate { get; set; } = default!;
    [JsonPropertyName("due_date")] public DateTime DueDate { get; set; } = default!;
    [JsonPropertyName("priority_level")] public PriorityLevel PriorityLevel { get; set; }
    [JsonPropertyName("job_level")] public JobType JobType { get; set; }
    [JsonPropertyName("quantities")] public Quantities Quantities { get; set; } = default!;
    [JsonPropertyName("metrics")] public Metrics Metrics { get; set; } = default!;
    [JsonPropertyName("schedule")] public JobSchedule Schedule { get; set; } = default!;
    [JsonPropertyName("setup")] public SetupPhase Setup { get; set; } = default!;
    [JsonPropertyName("downtime_events")] public List<DowntimeEvent> DowntimeEvents { get; set; } = default!;
    [JsonPropertyName("dependencies")] public List<string> Dependencies { get; set; } = default!;

    [JsonPropertyName("attachments")] public List<string> Attachments { get; set; } = new();
    [JsonPropertyName("created_at")] public DateTime CreatedAt { get; set; }
    [JsonPropertyName("updated_at")] public DateTime UpdatedAt { get; set; }
}

public class Quantities
{
    [JsonPropertyName("required")] public int Required { get; set; }
    [JsonPropertyName("completed")] public int Completed { get; set; }
    [JsonPropertyName("good")] public int Good { get; set; }
    [JsonPropertyName("bad")] public int Bad { get; set; }
    [JsonPropertyName("in_progress")] public int InProgress { get; set; }
}

public class Metrics
{
    [JsonPropertyName("target_cycle_time")] public float TargetCycleTime { get; set; }
    [JsonPropertyName("scheduled_time_seconds")] public float ScheduledTimeSeconds { get; set; }  // excluded
}

public class JobSchedule
{
    [JsonPropertyName("planned_start")] public DateTime PlannedStart { get; set; }
    [JsonPropertyName("planned_end")] public DateTime PlannedEnd { get; set; }
}

public class SetupPhase
{
    [JsonPropertyName("start_time")] public DateTime StartTime { get; set; }
    [JsonPropertyName("end_time")] public DateTime EndTime { get; set; }
}

public class DowntimeEvent
{
    [JsonPropertyName("reason")] public DowntimeReason Reason { get; set; }
    [JsonPropertyName("start_time")] public DateTime StartTime { get; set; }
    [JsonPropertyName("end_time")] public DateTime EndTime { get; set; }
    [JsonPropertyName("duration")] public float Duration { get; set; }
}