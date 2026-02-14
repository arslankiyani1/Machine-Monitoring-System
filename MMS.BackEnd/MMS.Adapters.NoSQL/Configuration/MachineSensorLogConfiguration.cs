namespace MMS.Adapters.NoSQL.Configuration;

public class MachineSensorLogConfiguration : IEntityTypeConfiguration<MachineSensorLog>
{
    public void Configure(EntityTypeBuilder<MachineSensorLog> builder)
    {
        builder.ToContainer("MachineSensorData");
        builder.HasPartitionKey(x => x.MachineId);
        builder.Property(x => x.Id).ToJsonProperty("id");
        builder.Property(x => x.MachineId).ToJsonProperty("machine_id");
        builder.Property(x => x.SensorId).ToJsonProperty("sensor_id");
        builder.Property(x => x.DateTime).ToJsonProperty("date_time");

        builder.Property(x => x.SensorStatus)
            .HasConversion<string>() // store enum as string
            .ToJsonProperty("sensor_status");

        var jsonValueConverter = new ValueConverter<JsonElement, string>(
            v => JsonElementToString(v),
            v => StringToJsonElement(v)
        );

        // ✅ Mapping for SensorReading (owned collection)
        builder.OwnsMany(x => x.SensorReading, sa =>
        {
            sa.ToJsonProperty("sensor_reading");

            sa.Property(p => p.Key)
                .HasConversion<string>()
                .ToJsonProperty("key");

            // ✅ store as float in Cosmos
            sa.Property(p => p.Value)
                .HasConversion<float>()
                .ToJsonProperty("value");

            sa.Property(p => p.Unit)
                    .HasConversion<string>()
                    .ToJsonProperty("unit");
        });

        builder.HasNoDiscriminator();
    }

    private static JsonElement StringToJsonElement(string jsonString)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(jsonString))
            {
                return JsonDocument.Parse("\"\"").RootElement; // empty string JSON
            }

            // If it's not valid JSON, wrap it as a JSON string
            var trimmed = jsonString.Trim();
            if (!(trimmed.StartsWith("{") || trimmed.StartsWith("[") || trimmed.StartsWith("\"") ||
                  trimmed == "true" || trimmed == "false" || double.TryParse(trimmed, out _)))
            {
                // Wrap in quotes if it's raw text like: success, on, etc.
                trimmed = JsonSerializer.Serialize(trimmed);
            }

            return JsonDocument.Parse(trimmed).RootElement;
        }
        catch
        {
            return JsonDocument.Parse("\"\"").RootElement; // fallback empty string
        }
    }

    private static string JsonElementToString(JsonElement jsonElement)
    {
        return jsonElement.ToString();
    }
}