namespace MMS.Adapter.AzureWebJobs.WebJobs;

public class HistoricalStatsJob(ILogger<HistoricalStatsJob> _logger,
    IServiceScopeFactory _scopeFactory,
    IOptions<HistoricalStatsJobOptions> options) : BackgroundService
{
    private readonly TimeSpan _runTimeUtc = options.Value.RunTimeUtc;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🚀 HistoricalStatsJob service started at {time}", DateTime.UtcNow);

        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = GetNextRunDelay();
            var scheduledRunTime = DateTime.UtcNow.Add(delay);

            _logger.LogInformation("⏳ Next job scheduled at {nextRun}", scheduledRunTime);

            try
            {
                await Task.Delay(delay, stoppingToken);

                // ✅ Calculate target date AFTER waking up to ensure we process the correct "yesterday"
                var targetDate = DateTime.UtcNow.Date.AddDays(-1);

                await RunJobAsync(targetDate, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                _logger.LogInformation("🛑 HistoricalStatsJob stopped gracefully.");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error while running job");
            }
        }
    }

    private async Task RunJobAsync(DateTime targetDate, CancellationToken token)
    {
        _logger.LogInformation("=== 📝 Job started at {time} for {date} ===",
            DateTime.UtcNow, targetDate.ToString("yyyy-MM-dd"));

        using var scope = _scopeFactory.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IHistoricalStatsService>();

        var response = await service.CreateHistoricalRecordForDayAsync(targetDate);

        if (response.StatusCode == StatusCodes.Status201Created)
        {
            _logger.LogInformation("✅ Historical stats created for {date} at {time}",
                targetDate.ToString("yyyy-MM-dd"), DateTime.UtcNow);
        }
        else
        {
            _logger.LogWarning("⚠️ Job for {date} returned {status}: {message}",
                targetDate.ToString("yyyy-MM-dd"), response.StatusCode, response.Message);
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