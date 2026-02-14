namespace MMS.Adapter.AzureRedisCache.Implementations;

/// <summary>
/// JSON-based cache serializer (Dependency Inversion Principle)
/// </summary>
public class JsonCacheSerializer : Abstractions.ICacheSerializer
{
    public string Serialize<T>(T value) => JsonSerializer.Serialize(value);

    public T? Deserialize<T>(string value) => JsonSerializer.Deserialize<T>(value);
}
