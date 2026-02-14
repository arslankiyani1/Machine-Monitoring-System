namespace MMS.Application.Interfaces;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, string? prefix = null);
    Task RemoveAsync(string key);
    Task RemoveTrackedKeysAsync(string prefix);
    Task RemoveByPrefixAsync(string v);
    Task SetMachineHeartbeatAsync(string machineId);
    //Task TriggerImmediateHeartbeatAsync (string machineId, DateTime? endTimeUtc = null);
    Task CancelMachineHeartbeatAsync(string v);
}
