namespace MMS.Adapter.AzureRedisCache.Abstractions;

/// <summary>
/// Abstraction for Redis database operations (Dependency Inversion Principle)
/// </summary>
public interface IRedisDatabaseAdapter
{
    Task<RedisValue> StringGetAsync(RedisKey key);
    Task<bool> StringSetAsync(RedisKey key, RedisValue value, TimeSpan? expiry = null, When when = When.Always);
    Task<bool> KeyDeleteAsync(RedisKey key);
    Task<bool> KeyExistsAsync(RedisKey key);
    Task<RedisValue[]> SetMembersAsync(RedisKey key);
    Task<bool> SetAddAsync(RedisKey key, RedisValue value);
    Task<RedisResult> ScriptEvaluateAsync(string script, RedisKey[] keys, RedisValue[] values);
    IBatch CreateBatch();
}
