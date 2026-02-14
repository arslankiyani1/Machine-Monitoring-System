namespace MMS.Adapter.AzureRedisCache.Implementations;

/// <summary>
/// Service for tracking keys by path/prefix (Single Responsibility Principle)
/// </summary>
public class RedisKeyTracker : Abstractions.IKeyTracker
{
    private readonly Abstractions.IRedisDatabaseAdapter _database;
    private readonly Abstractions.IRedisServerAdapter _server;
    private readonly ILogger<RedisKeyTracker> _logger;
    private const string TrackedKeyPrefix = "tracked:";

    public RedisKeyTracker(
        Abstractions.IRedisDatabaseAdapter database,
        Abstractions.IRedisServerAdapter server,
        ILogger<RedisKeyTracker> logger)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
        _server = server ?? throw new ArgumentNullException(nameof(server));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task TrackKeyAsync(string path, string key)
    {
        if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(key)) return;
        
        string setKey = $"{TrackedKeyPrefix}{path}";
        await _database.SetAddAsync(setKey, key).ConfigureAwait(false);
    }

    public async Task RemoveTrackedKeysAsync(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return;
        
        string setKey = $"{TrackedKeyPrefix}{path}";
        var keys = await _database.SetMembersAsync(setKey).ConfigureAwait(false);
        
        // Use batch deletion with pipeline for better performance
        if (keys.Length > 0)
        {
            var batch = _database.CreateBatch();
            var deleteTasks = new List<Task<bool>>();
            
            foreach (var key in keys)
            {
                try
                {
                    deleteTasks.Add(batch.KeyDeleteAsync(key.ToString()));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error queuing tracked key {Key} for deletion for path {Path}", key, path);
                }
            }
            
            batch.Execute();
            await Task.WhenAll(deleteTasks).ConfigureAwait(false);
        }
        
        await _database.KeyDeleteAsync(setKey).ConfigureAwait(false);
    }

    public async Task RemoveByPrefixAsync(string prefix)
    {
        if (string.IsNullOrWhiteSpace(prefix)) return;
        
        var setKey = $"{TrackedKeyPrefix}{prefix}";
        if (await _database.KeyExistsAsync(setKey).ConfigureAwait(false))
        {
            var members = await _database.SetMembersAsync(setKey).ConfigureAwait(false);
            
            // Use batch deletion with pipeline
            if (members.Length > 0)
            {
                var batch = _database.CreateBatch();
                var deleteTasks = members.Select(k => batch.KeyDeleteAsync(k.ToString())).ToList();
                batch.Execute();
                await Task.WhenAll(deleteTasks).ConfigureAwait(false);
            }
            
            await _database.KeyDeleteAsync(setKey).ConfigureAwait(false);
            return;
        }
        
        // Fallback to pattern-based deletion
        var keysToDelete = new List<RedisKey>();
        foreach (var key in _server.GetKeys($"{prefix}*"))
        {
            keysToDelete.Add(key);
        }
        
        if (keysToDelete.Count > 0)
        {
            var batch = _database.CreateBatch();
            var deleteTasks = keysToDelete.Select(key => 
            {
                try
                {
                    return batch.KeyDeleteAsync(key);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error queuing key {Key} for deletion by prefix {Prefix}", key, prefix);
                    return Task.FromResult(false);
                }
            }).ToList();
            
            batch.Execute();
            await Task.WhenAll(deleteTasks).ConfigureAwait(false);
        }
    }
}
