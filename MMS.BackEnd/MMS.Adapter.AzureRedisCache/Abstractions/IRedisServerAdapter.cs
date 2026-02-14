namespace MMS.Adapter.AzureRedisCache.Abstractions;

/// <summary>
/// Abstraction for Redis server operations (Dependency Inversion Principle)
/// </summary>
public interface IRedisServerAdapter
{
    IEnumerable<RedisKey> GetKeys(string pattern);
}
