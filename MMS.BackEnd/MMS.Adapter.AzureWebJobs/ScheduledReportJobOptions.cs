namespace MMS.Adapter.AzureWebJobs;

public class ScheduledReportJobOptions
{
    public TimeSpan RunTimeUtc { get; set; } = TimeSpan.FromHours(6); // Default 6:00 AM UTC
}
