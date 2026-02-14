namespace MMS.Adapter.AzureRedisCache.Implementations;

/// <summary>
/// Adapter for Redis database operations (Dependency Inversion Principle)
/// </summary>
public class RedisDatabaseAdapter : Abstractions.IRedisDatabaseAdapter
{
    private readonly IDatabase _database;

    public RedisDatabaseAdapter(IDatabase database)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
    }

    public Task<RedisValue> StringGetAsync(RedisKey key) => _database.StringGetAsync(key);
    
    public Task<bool> StringSetAsync(RedisKey key, RedisValue value, TimeSpan? expiry = null, When when = When.Always)
        => _database.StringSetAsync(key, value, expiry, when);
    
    public Task<bool> KeyDeleteAsync(RedisKey key) => _database.KeyDeleteAsync(key);
    
    public Task<bool> KeyExistsAsync(RedisKey key) => _database.KeyExistsAsync(key);
    
    public Task<RedisValue[]> SetMembersAsync(RedisKey key) => _database.SetMembersAsync(key);
    
    public Task<bool> SetAddAsync(RedisKey key, RedisValue value) => _database.SetAddAsync(key, value);
    
    public Task<RedisResult> ScriptEvaluateAsync(string script, RedisKey[] keys, RedisValue[] values)
        => _database.ScriptEvaluateAsync(script, keys, values);
    
    public IBatch CreateBatch() => _database.CreateBatch();
}
