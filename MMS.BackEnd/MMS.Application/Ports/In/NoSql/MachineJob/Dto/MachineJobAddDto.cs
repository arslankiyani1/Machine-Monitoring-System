namespace MMS.Application.Ports.In.NoSql.MachineJob.Dto;

public record MachineJobAddDto(
    Guid CustomerId,
    List<string> MachineIds,
    List<string> MachineNames,
    Guid OperatorId,
    string JobName,
    string OperatorName,
    JobStatus Status,
    DateTime StartTime,
    DateTime EndTime,
    string PartNumber,
    string ProgramNo,
    string MainProgram,
    string Description,
    string OrderNo,
    DateTime OrderDate,
    DateTime DueDate,
    PriorityLevel PriorityLevel,
    JobType JobType,
    QuantitiesDto Quantities,
    MetricsDto Metrics,
    JobScheduleDto Schedule,
    SetupPhaseDto Setup,
    List<DowntimeEventDto> DowntimeEvents,
    List<string> Dependencies,
    List<string> Attachments,
    string Notes,
    IFormFile? File
)
{
    public MachineJobAddDto() : this(
    Guid.Empty,                   // CustomerId
    new List<string>(),           // MachineIds
    new List<string>(),           // ✅ MachineNames (missing in your version)
    Guid.Empty,                   // OperatorId
    string.Empty,                 // JobId
    string.Empty,                 // OperatorName
    JobStatus.Scheduled,          // Status
    DateTime.UtcNow,              // StartTime
    DateTime.UtcNow,              // EndTime
    string.Empty,                 // PartNumber
    string.Empty,                 // ProgramNo
    string.Empty,                 // MainProgram
    string.Empty,                 // Description
    string.Empty,                 // OrderNo
    DateTime.UtcNow,              // OrderDate
    DateTime.UtcNow,              // DueDate
    PriorityLevel.Low,            // PriorityLevel
    JobType.Production,           // JobType
    new QuantitiesDto(),          // Quantities
    new MetricsDto(),             // Metrics
    new JobScheduleDto(),         // Schedule
    new SetupPhaseDto(),          // Setup
    new List<DowntimeEventDto>(), // DowntimeEvents
    new List<string>(),           // Dependencies
    new List<string>(),           // Attachments
    string.Empty,                 // Notes
    null                          // File
)
    { }

}

public record MachineJobUpdateDto(
    string Id,
    Guid CustomerId,
    List<string> MachineIds,
    List<string> MachineNames,
    Guid OperatorId,
    string OperatorName,
    string JobName,
    JobStatus Status,
    DateTime StartTime,
    DateTime EndTime,
    string PartNumber,
    string ProgramNo,
    string MainProgram,
    string Description,
    string OrderNo,
    DateTime OrderDate,
    DateTime DueDate,
    PriorityLevel PriorityLevel,
    JobType JobType,
    QuantitiesDto Quantities,
    MetricsDto Metrics,
    JobScheduleDto Schedule,
    SetupPhaseDto Setup,
    List<DowntimeEventDto> DowntimeEvents,
    List<string> Dependencies,
    List<string> Attachments
    //string Notes,
    //IFormFile? File
)
{
    public MachineJobUpdateDto() : this(
        string.Empty,
        Guid.Empty,
        new List<string>(),
        new List<string>(),
        Guid.Empty,
        string.Empty,
        string.Empty,
        JobStatus.Scheduled,
        DateTime.UtcNow,
        DateTime.UtcNow,
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        DateTime.UtcNow,
        DateTime.UtcNow,
        PriorityLevel.Low,
        JobType.Production,
        new QuantitiesDto(),
        new MetricsDto(),
        new JobScheduleDto(),
        new SetupPhaseDto(),
        new List<DowntimeEventDto>(),
        new List<string>(),
        new List<string>()
        //string.Empty,
        //null
    )
    { }
}
public record MachineJobDto(
    string Id,
    string JobName,
    Guid CustomerId,
    List<string> MachineIds,
    List<string> MachineNames,
    Guid OperatorId,
    string OperatorName,
    JobStatus Status,
    DateTime StartTime,
    DateTime EndTime,
    string PartNumber,
    string ProgramNo,
    string MainProgram,
    string Description,
    string OrderNo,
    DateTime OrderDate,
    DateTime DueDate,
    PriorityLevel PriorityLevel,
    JobType JobType,
    QuantitiesDto Quantities,
    MetricsDto Metrics,
    JobScheduleDto Schedule,
    SetupPhaseDto Setup,
    List<DowntimeEventDto> DowntimeEvents,
    List<string> Dependencies,
    List<string> Attachments,
    DateTime CreatedAt,
    DateTime UpdatedAt
)
{
    public MachineJobDto() : this(
        string.Empty,
        string.Empty,
        Guid.Empty,
        new List<string>(),
        new List<string>(),
        Guid.Empty,
        string.Empty,
        JobStatus.Scheduled,
        DateTime.UtcNow,
        DateTime.UtcNow,
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        DateTime.UtcNow,
        DateTime.UtcNow,
        PriorityLevel.Low,
        JobType.Production,
        new QuantitiesDto(),
        new MetricsDto(),
        new JobScheduleDto(),
        new SetupPhaseDto(),
        new List<DowntimeEventDto>(),
        new List<string>(),
        new List<string>(),
        DateTime.UtcNow,
        DateTime.UtcNow
    )
    { }
}

// ✅ Job status summary (no changes)
public class JobStatusSummaryDto
{
    public int TotalJobs { get; set; }
    public int Scheduled { get; set; }
    public int InProgress { get; set; }
    public int Interrupted { get; set; }
    public int Completed { get; set; }
}

// ✅ Quantities DTO
public record QuantitiesDto(
    int Required,
    int Completed,
    int Good,
    int Bad,
    int InProgress,
    int PartsPerCycle
)
{
    public QuantitiesDto() : this(0, 0, 0, 0, 0, 0) { }
}

// ✅ Metrics DTO
public record MetricsDto(
    float TargetCycleTime,
    float ScheduledTimeSeconds
)
{
    public MetricsDto() : this(0, 0) { }
}

// ✅ Schedule DTO
public record JobScheduleDto(
    DateTime PlannedStart,
    DateTime PlannedEnd
)
{
    public JobScheduleDto() : this(DateTime.UtcNow, DateTime.UtcNow) { }
}

// ✅ Setup DTO
public record SetupPhaseDto(
    DateTime StartTime,
    DateTime EndTime
)
{
    public SetupPhaseDto() : this(DateTime.UtcNow, DateTime.UtcNow) { }
}

// ✅ Downtime event DTO — using correct enum type
public record DowntimeEventDto(
    DowntimeReason Reason,
    DateTime StartTime,
    DateTime EndTime,
    float Duration
)
{
    public DowntimeEventDto() : this(DowntimeReason.Maintenance, DateTime.UtcNow, DateTime.UtcNow, 0) { }
}

