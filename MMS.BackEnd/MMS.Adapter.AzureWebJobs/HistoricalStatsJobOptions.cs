namespace MMS.Adapter.AzureWebJobs;

public class HistoricalStatsJobOptions
{
    public TimeSpan RunTimeUtc { get; set; } = TimeSpan.FromHours(2); // Default = 2:00 AM
}
