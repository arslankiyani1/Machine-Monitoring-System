namespace MMS.Application.Models.SQL;

public class Notification : Trackable
{
    public string Title { get; set; } = default!;
    public string Body { get; set; } = default!;
    public List<string> Recipients { get; set; } = new();
    public Guid? MachineId { get; set; }
    public string? MachineName { get; set; }
    public Guid? CustomerId { get; set; }
    public string? Priority { get; set; }
    public string? Link { get; set; }
    public NotificationStatus ReadStatus { get; set; } = NotificationStatus.Unread;
    public Enum.NotificationTypes NotificationTypes { get; set; }
    public DateTime? ReadAt { get; set; }

    // Keep UserId for tracking who received it (can be populated per recipient)
    public Guid? UserId { get; set; }
}