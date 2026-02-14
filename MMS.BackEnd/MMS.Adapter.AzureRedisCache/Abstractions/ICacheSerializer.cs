namespace MMS.Adapter.AzureRedisCache.Abstractions;

/// <summary>
/// Interface for cache serialization (Dependency Inversion Principle)
/// </summary>
public interface ICacheSerializer
{
    string Serialize<T>(T value);
    T? Deserialize<T>(string value);
}
