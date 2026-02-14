namespace MMS.Adapter.AzureWebJobs.WebJobs;

public class ScheduledReportJob(
    ILogger<ScheduledReportJob> logger,
    IServiceScopeFactory scopeFactory,
    IOptions<ScheduledReportJobOptions> options) : BackgroundService
{
    private readonly TimeSpan _runTimeUtc = options.Value.RunTimeUtc;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("🚀 ScheduledReportJob service started at {time}", DateTime.UtcNow);

        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = GetNextRunDelay();
            var scheduledRunTime = DateTime.UtcNow.Add(delay);

            logger.LogInformation("⏳ Next scheduled report run at {nextRun}", scheduledRunTime);

            try
            {
                await Task.Delay(delay, stoppingToken);

                //Calculate the run date (today in UTC)
                var runDate = DateTime.UtcNow.Date;

                await RunJobAsync(runDate, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                logger.LogInformation("🛑 ScheduledReportJob stopped gracefully.");
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Error while running ScheduledReportJob");
            }
        }
    }

    private async Task RunJobAsync(DateTime runDate, CancellationToken token)
    {
        logger.LogInformation("=== 📝 ScheduledReportJob started at {time} for date {date} ===",
            DateTime.UtcNow, runDate.ToString("yyyy-MM-dd"));

        using var scope = scopeFactory.CreateScope();
        var scheduledReportService = scope.ServiceProvider.GetRequiredService<IScheduledReportService>();

        try
        {
            // Get all due report settings
            var dueSettings = await scheduledReportService.GetDueReportSettingsAsync(runDate);
            var settingsList = dueSettings.ToList();

            logger.LogInformation("📋 Found {count} report settings due for execution", settingsList.Count);

            int successCount = 0;
            int failureCount = 0;

            foreach (var setting in settingsList)
            {
                if (token.IsCancellationRequested)
                {
                    logger.LogWarning("⚠️ Job cancellation requested, stopping early");
                    break;
                }

                try
                {
                    logger.LogInformation("📊 Processing report: {ReportName} (ID: {Id}, Frequency: {Frequency})",
                        setting.ReportName, setting.Id, setting.Frequency);

                    var success = await scheduledReportService.GenerateAndSendReportAsync(setting, runDate);

                    if (success)
                    {
                        successCount++;
                        logger.LogInformation("✅ Successfully processed report: {ReportName}", setting.ReportName);
                    }
                    else
                    {
                        failureCount++;
                        logger.LogWarning("⚠️ Failed to process report: {ReportName}", setting.ReportName);
                    }
                }
                catch (Exception ex)
                {
                    failureCount++;
                    logger.LogError(ex, "❌ Exception while processing report: {ReportName}", setting.ReportName);
                }
            }

            logger.LogInformation("=== 📝 ScheduledReportJob completed. Success: {success}, Failures: {failures} ===",
                successCount, failureCount);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Failed to get due report settings");
        }
    }

    private TimeSpan GetNextRunDelay()
    {
        var now = DateTime.UtcNow;
        var nextRun = now.Date.Add(_runTimeUtc);

        if (nextRun <= now)
        {
            nextRun = nextRun.AddDays(1); // Move to tomorrow if already passed
        }

        return nextRun - now;
    }
}
