using System.Text.Json;
using System.Text.Json.Serialization;

namespace MMS.WebAPI.Extension;

public static class CosmosServiceExtensions
{
    public static IServiceCollection AddCosmosClient(this IServiceCollection services, IConfiguration configuration)
    {
        var endpoint = configuration["Cosmos:EndpointUri"];
        var key = configuration["Cosmos:PrimaryKey"];

        var cosmosOptions = new CosmosClientOptions
        {
            RequestTimeout = TimeSpan.FromSeconds(30),
            MaxRetryAttemptsOnRateLimitedRequests = 5,
            MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(30),
            AllowBulkExecution = true,
            ConnectionMode = ConnectionMode.Direct
        };

        services.AddSingleton(_ => new CosmosClient(endpoint!, key!, cosmosOptions));
        return services;
    }
}

public class JsonDateTime24HourConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader,
                                  Type typeToConvert,
                                  JsonSerializerOptions options)
        => DateTime.Parse(reader.GetString()!);
    public override void Write(Utf8JsonWriter writer,
                               DateTime value,
                               JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString("yyyy-MM-dd HH:mm:ss"));
}