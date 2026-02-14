namespace MMS.Adapter.AzureRedisCache.Abstractions;

/// <summary>
/// Interface for key tracking operations (Interface Segregation Principle)
/// </summary>
public interface IKeyTracker
{
    Task TrackKeyAsync(string path, string key);
    Task RemoveTrackedKeysAsync(string path);
    Task RemoveByPrefixAsync(string prefix);
}
