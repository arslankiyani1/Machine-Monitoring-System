namespace MMS.Adapter.AzureRedisCache.Implementations;

/// <summary>
/// Service for machine heartbeat operations (Single Responsibility Principle)
/// </summary>
public class RedisMachineHeartbeatService : Abstractions.IMachineHeartbeatService
{
    private readonly Abstractions.IRedisDatabaseAdapter _database;
    private readonly TimeSpan _defaultHeartbeatTtl;

    public RedisMachineHeartbeatService(
        Abstractions.IRedisDatabaseAdapter database,
        TimeSpan? defaultHeartbeatTtl = null)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
        _defaultHeartbeatTtl = defaultHeartbeatTtl ?? TimeSpan.FromMinutes(3);
    }

    public async Task SetMachineHeartbeatAsync(string machineId)
    {
        if (string.IsNullOrWhiteSpace(machineId))
            throw new ArgumentException("Machine ID cannot be null", nameof(machineId));

        var key = $"machine:heartbeat:{machineId}";
        // Always refresh - Redis SETEX is very cheap
        await _database.StringSetAsync(key, "active", _defaultHeartbeatTtl).ConfigureAwait(false);
    }

    public Task CancelMachineHeartbeatAsync(string machineId)
    {
        if (string.IsNullOrWhiteSpace(machineId))
            throw new ArgumentException("Machine ID cannot be null", nameof(machineId));

        return _database.KeyDeleteAsync($"machine:heartbeat:{machineId}");
    }
}
