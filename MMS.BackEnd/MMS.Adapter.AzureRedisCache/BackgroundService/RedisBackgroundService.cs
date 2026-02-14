using System.Threading.Tasks;

namespace MMS.Adapter.AzureRedisCache.BackgroundService;

public class RedisBackgroundService : Microsoft.Extensions.Hosting.BackgroundService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RedisBackgroundService> _logger;

    public RedisBackgroundService(
        IConnectionMultiplexer redis,
        IServiceScopeFactory scopeFactory,
        ILogger<RedisBackgroundService> logger)
    {
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var subscriber = _redis.GetSubscriber();
        var channel = new RedisChannel("__keyevent@0__:expired", RedisChannel.PatternMode.Literal);

        await subscriber.SubscribeAsync(channel, async (_, expiredKey) =>
        {
            if (stoppingToken.IsCancellationRequested)
                return;

            HandleExpiredKey(expiredKey);
        });

        _logger.LogInformation("Redis expiration listener started on DB 0.");

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Redis expiration listener stopping...");
        }
        finally
        {
            await subscriber.UnsubscribeAsync(channel);
        }
    }

    private async Task HandleExpiredKey(RedisValue expiredKey)
    {
        string key = expiredKey.ToString();

        // Only process machine heartbeats
        if (!key.StartsWith("machine:heartbeat:"))
            return;

        var machineId = key.Replace("machine:heartbeat:", "");

        if (!Guid.TryParse(machineId, out Guid machineGuid))
        {
            _logger.LogWarning("Invalid machine heartbeat key expired: {Key}", key);
            return;
        }

       await ProcessMachineOffline(machineGuid);
    }

    private async Task ProcessMachineOffline(Guid machineId)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var monitor = scope.ServiceProvider.GetRequiredService<IMachineMonitoringService>();

            await monitor.MarkMachineOfflineAsync(machineId);

            _logger.LogInformation("Machine offline event processed for MachineId={MachineId}", machineId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error while processing machine offline event for MachineId={MachineId}",
                machineId);
        }
    }
}
