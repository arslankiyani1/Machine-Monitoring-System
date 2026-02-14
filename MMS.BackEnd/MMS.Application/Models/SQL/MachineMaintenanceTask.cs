namespace MMS.Application.Models.SQL;

public class MachineMaintenanceTask : Trackable
{
    public string MaintenanceTaskName { get; set; } = default!;
    public string? Reason { get; set; } = default!;
    public string? Notes { get; set; }
    public bool IsFinished { get; set; } = false;
    public List<string> Attachments { get; set; } =  new();
    public MaintenanceTaskCategory Category { get; set; }
    public PriorityLevel Priority { get; set; } = PriorityLevel.Medium;

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public DateTime ScheduledDate { get; set; }
    public DateTime DueDate { get; set; }

    // Assignment
    public Guid? AssignedToUserId { get; set; }
    public string? AssignedToUserName { get; set; }

    public Guid? JobId { get; set; }
    public Guid MachineId { get; set; }
    public Guid CustomerId { get; set; }
    public Machine Machine { get; set; } = default!;
    public Customer Customer { get; set; } = default!;

}