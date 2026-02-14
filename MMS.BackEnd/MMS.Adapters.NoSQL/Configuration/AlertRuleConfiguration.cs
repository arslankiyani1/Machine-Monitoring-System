using System.Text.Json;
using System.Text.Json.Serialization;

namespace MMS.Adapters.NoSQL.Configuration;

public class AlertRuleConfiguration : IEntityTypeConfiguration<AlertRule>
{
    public void Configure(EntityTypeBuilder<AlertRule> builder)
    {
        builder.ToContainer(Constant.AlertRule);
        builder.HasPartitionKey(x => x.CustomerId);
        builder.HasNoDiscriminator();

        // Primary Properties
        builder.Property(x => x.Id)
            .ToJsonProperty("id");
        builder.Property(x => x.CustomerId)
            .ToJsonProperty("customer_id");
        builder.Property(x => x.MachineId)
            .ToJsonProperty("machine_id");
        builder.Property(x => x.SensorId)
            .ToJsonProperty("sensor_id");
        builder.Property(x => x.RuleName)
            .ToJsonProperty("rule_name");
        builder.Property(x => x.Enabled)
            .ToJsonProperty("enabled");
        builder.Property(x => x.CreatedAt)
            .ToJsonProperty("created_at");
        builder.Property(x => x.UpdatedAt)
            .ToJsonProperty("updated_at");
        builder.Property(x => x.LastTriggered)
            .ToJsonProperty("last_triggered");

        // Enum Properties with String Conversion
        builder.Property(x => x.AlertScope)
            .ToJsonProperty("alert_scope")
            .HasConversion<string>();
        builder.Property(x => x.Logic)
            .ToJsonProperty("logic")
            .HasConversion<string>();
        builder.Property(x => x.Priority)
            .ToJsonProperty("priority")
            .HasConversion<string>();

        // ✅ Conditions - Store as JSON array
        builder.Property(x => x.Conditions)
            .ToJsonProperty("conditions")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonSerializerOptions),
                v => JsonSerializer.Deserialize<List<Condition>>(v, JsonSerializerOptions) ?? new List<Condition>()
            );

        // ✅ AlertActions - Store as JSON array
        builder.Property(x => x.AlertActions)
            .ToJsonProperty("alertactions")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonSerializerOptions),
                v => JsonSerializer.Deserialize<List<AlertAction>>(v, JsonSerializerOptions) ?? new List<AlertAction>()
            );
    }

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        WriteIndented = false,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
}