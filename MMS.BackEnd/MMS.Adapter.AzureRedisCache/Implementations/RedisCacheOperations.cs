namespace MMS.Adapter.AzureRedisCache.Implementations;

/// <summary>
/// Core cache operations service (Single Responsibility Principle)
/// </summary>
public class RedisCacheOperations
{
    private readonly Abstractions.IRedisDatabaseAdapter _database;
    private readonly Abstractions.ICacheSerializer _serializer;

    public RedisCacheOperations(
        Abstractions.IRedisDatabaseAdapter database,
        Abstractions.ICacheSerializer serializer)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return default;
        
        var value = await _database.StringGetAsync(key).ConfigureAwait(false);
        if (value.IsNullOrEmpty) return default;
        
        return _serializer.Deserialize<T>(value!);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException(nameof(key));
        
        var serialized = _serializer.Serialize(value);
        await _database.StringSetAsync(key, serialized, expiration ?? TimeSpan.FromHours(1)).ConfigureAwait(false);
    }

    public async Task RemoveAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return;
        await _database.KeyDeleteAsync(key).ConfigureAwait(false);
    }
}
