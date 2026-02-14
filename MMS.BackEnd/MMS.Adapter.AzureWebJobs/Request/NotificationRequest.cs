namespace MMS.Adapter.AzureWebJobs.Request;

public record NotificationRequest
{
    public string Message { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime SentAt { get; init; }
    public Guid UserId { get; init; }
}
