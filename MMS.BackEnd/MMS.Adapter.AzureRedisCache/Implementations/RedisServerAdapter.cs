namespace MMS.Adapter.AzureRedisCache.Implementations;

/// <summary>
/// Adapter for Redis server operations (Dependency Inversion Principle)
/// </summary>
public class RedisServerAdapter : Abstractions.IRedisServerAdapter
{
    private readonly IServer? _server;

    public RedisServerAdapter(IConnectionMultiplexer redis)
    {
        if (redis == null) throw new ArgumentNullException(nameof(redis));
        var endpoints = redis.GetEndPoints();
        _server = endpoints.Length > 0 ? redis.GetServer(endpoints[0]) : null;
    }

    public IEnumerable<RedisKey> GetKeys(string pattern)
    {
        if (_server == null) return Enumerable.Empty<RedisKey>();
        return _server.Keys(pattern: pattern);
    }
}
