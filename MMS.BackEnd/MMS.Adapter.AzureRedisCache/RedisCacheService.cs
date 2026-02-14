namespace MMS.Adapter.AzureRedisCache;

/// <summary>
/// Redis cache service implementation following SOLID principles
/// - Single Responsibility: Delegates to specialized services
/// - Open/Closed: Extensible through interfaces
/// - Liskov Substitution: Properly implements ICacheService
/// - Interface Segregation: Uses focused interfaces
/// - Dependency Inversion: Depends on abstractions
/// </summary>
public class RedisCacheService : ICacheService
{
    private readonly Implementations.RedisCacheOperations _cacheOperations;
    private readonly Abstractions.IKeyTracker _keyTracker;
    private readonly Abstractions.IMachineHeartbeatService _heartbeatService;

    public RedisCacheService(
        Implementations.RedisCacheOperations cacheOperations,
        Abstractions.IKeyTracker keyTracker,
        Abstractions.IMachineHeartbeatService heartbeatService)
    {
        _cacheOperations = cacheOperations ?? throw new ArgumentNullException(nameof(cacheOperations));
        _keyTracker = keyTracker ?? throw new ArgumentNullException(nameof(keyTracker));
        _heartbeatService = heartbeatService ?? throw new ArgumentNullException(nameof(heartbeatService));
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        return await _cacheOperations.GetAsync<T>(key).ConfigureAwait(false);
    }

    public async Task RemoveAsync(string key)
    {
        await _cacheOperations.RemoveAsync(key).ConfigureAwait(false);
    }

    public async Task RemoveByPrefixAsync(string prefix)
    {
        await _keyTracker.RemoveByPrefixAsync(prefix).ConfigureAwait(false);
    }

    public async Task RemoveTrackedKeysAsync(string path)
    {
        await _keyTracker.RemoveTrackedKeysAsync(path).ConfigureAwait(false);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, string? path = null)
    {
        await _cacheOperations.SetAsync(key, value, expiration).ConfigureAwait(false);
        if (!string.IsNullOrEmpty(path))
        {
            await _keyTracker.TrackKeyAsync(path, key).ConfigureAwait(false);
        }
    }

    public async Task SetMachineHeartbeatAsync(string machineId)
    {
        await _heartbeatService.SetMachineHeartbeatAsync(machineId).ConfigureAwait(false);
    }

    public Task CancelMachineHeartbeatAsync(string machineId)
    {
        return _heartbeatService.CancelMachineHeartbeatAsync(machineId);
    }
}