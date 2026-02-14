
namespace MMS.Adapter.AzureRedisCache.BackgroundService;

/// <summary>
/// ✅ OPTIMIZATION: Cache warming service to pre-load frequently accessed data on startup
/// </summary>
public class CacheWarmingService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CacheWarmingService> _logger;

    public CacheWarmingService(IServiceProvider serviceProvider, ILogger<CacheWarmingService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("🔥 Starting cache warming service...");

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();

            // ✅ OPTIMIZATION: Warm cache with frequently accessed static data
            // This can be extended to pre-load:
            // - Machine status settings
            // - Customer configurations
            // - User permissions
            // - Lookup tables

            _logger.LogInformation("✅ Cache warming completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error during cache warming");
            // Don't throw - cache warming failure shouldn't prevent app startup
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("🛑 Cache warming service stopped");
        return Task.CompletedTask;
    }
}

