namespace MMS.Adapter.AzureRedisCache.Abstractions;

/// <summary>
/// Interface for machine heartbeat operations (Interface Segregation Principle)
/// </summary>
public interface IMachineHeartbeatService
{
    Task SetMachineHeartbeatAsync(string machineId);
    Task CancelMachineHeartbeatAsync(string machineId);
}
