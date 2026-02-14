using MMS.Application.Interfaces;
using MMS.Adapter.AzureRedisCache.Abstractions;

namespace MMS.Adapter.AzureRedisCache;

/// <summary>
/// Distributed lock service following SOLID principles
/// - Single Responsibility: Handles only distributed locking
/// - Dependency Inversion: Depends on IRedisDatabaseAdapter abstraction
/// </summary>
public class DistributedLockService : IDistributedLockService
{
    private readonly Abstractions.IRedisDatabaseAdapter _database;
    private readonly ILogger<DistributedLockService> _logger;
    private const string LockPrefix = "lock:";

    public DistributedLockService(
        Abstractions.IRedisDatabaseAdapter database,
        ILogger<DistributedLockService> logger)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IDisposable?> AcquireLockAsync(string lockKey, TimeSpan expiry, TimeSpan? waitTime = null, CancellationToken cancellationToken = default)
    {
        var fullKey = $"{LockPrefix}{lockKey}";
        var lockValue = Guid.NewGuid().ToString();
        var waitUntil = waitTime.HasValue ? DateTime.UtcNow.Add(waitTime.Value) : DateTime.UtcNow;
        var retryDelay = TimeSpan.FromMilliseconds(50);

        while (DateTime.UtcNow < waitUntil && !cancellationToken.IsCancellationRequested)
        {
            // Try to acquire lock using SET with NX (only if not exists) and EX (expiry)
            var acquired = await _database.StringSetAsync(fullKey, lockValue, expiry, When.NotExists);
            
            if (acquired)
            {
                _logger.LogDebug("Acquired distributed lock: {LockKey}", lockKey);
                return new RedisLockHandle(_database, fullKey, lockValue, _logger);
            }

            // Wait before retrying
            await Task.Delay(retryDelay, cancellationToken);
        }

        _logger.LogWarning("Failed to acquire distributed lock: {LockKey} within timeout", lockKey);
        return null;
    }

    public async Task<(bool Success, IDisposable? LockHandle)> TryAcquireLockAsync(string lockKey, TimeSpan expiry)
    {
        var fullKey = $"{LockPrefix}{lockKey}";
        var lockValue = Guid.NewGuid().ToString();

        var acquired = await _database.StringSetAsync(fullKey, lockValue, expiry, When.NotExists);
        
        if (acquired)
        {
            var lockHandle = new RedisLockHandle(_database, fullKey, lockValue, _logger);
            return (true, lockHandle);
        }

        return (false, null);
    }

    /// <summary>
    /// Redis lock handle implementation (Single Responsibility Principle)
    /// </summary>
    private class RedisLockHandle : IDisposable
    {
        private readonly Abstractions.IRedisDatabaseAdapter _database;
        private readonly string _lockKey;
        private readonly string _lockValue;
        private readonly ILogger _logger;
        private bool _disposed = false;

        public RedisLockHandle(
            Abstractions.IRedisDatabaseAdapter database,
            string lockKey,
            string lockValue,
            ILogger logger)
        {
            _database = database;
            _lockKey = lockKey;
            _lockValue = lockValue;
            _logger = logger;
        }

        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                // Use Lua script to ensure we only delete our own lock
                const string script = @"
                    if redis.call('get', KEYS[1]) == ARGV[1] then
                        return redis.call('del', KEYS[1])
                    else
                        return 0
                    end";

                // Synchronous disposal - use GetAwaiter().GetResult() for proper async in sync context
                _database.ScriptEvaluateAsync(script, new RedisKey[] { _lockKey }, new RedisValue[] { _lockValue })
                    .GetAwaiter().GetResult();
                _logger.LogDebug("Released distributed lock: {LockKey}", _lockKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error releasing distributed lock: {LockKey}", _lockKey);
            }
            finally
            {
                _disposed = true;
            }
        }
    }
}

