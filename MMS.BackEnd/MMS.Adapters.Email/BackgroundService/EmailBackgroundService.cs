namespace MMS.Adapters.Email.BackgroundService;

[ExcludeFromCodeCoverage]
public class EmailBackgroundService(IEmailQueueService _emailQueueService, ILogger<EmailBackgroundService> _logger) :
    Microsoft.Extensions.Hosting.BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(LoggingMessages.EmailServiceStarting);
        while (!stoppingToken.IsCancellationRequested)
        {
            var emailTask = await _emailQueueService.DequeueEmailAsync(stoppingToken);
            try
            {
                await emailTask();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LoggingMessages.EmailServiceError);
            }
        }
        _logger.LogInformation(LoggingMessages.EmailServiceStopping);
    }
}