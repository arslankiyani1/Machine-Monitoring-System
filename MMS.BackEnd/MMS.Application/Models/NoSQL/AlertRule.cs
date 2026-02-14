using PriorityLevel = MMS.Application.Enum.PriorityLevel;

namespace MMS.Application.Models.NoSQL;

[Table("AlertRules")]
public class AlertRule
{
    [Key]
    [JsonPropertyName("id")] public string Id { get; set; } = default!;
    [JsonPropertyName("customer_id")] public Guid CustomerId { get; set; }
    [JsonPropertyName("sensor_id")] public Guid SensorId { get; set; }
    [JsonPropertyName("alert_scope")] public AlertScope AlertScope { get; set; }
    [JsonPropertyName("machine_id")] public Guid MachineId { get; set; } = default!;
    [JsonPropertyName("rule_name")] public string RuleName { get; set; } = default!;
    [JsonPropertyName("conditions")] public List<Condition> Conditions { get; set; } = default!;
    [JsonPropertyName("logic")] public Logic Logic { get; set; } = default!;
    [JsonPropertyName("alertactions")] public List<AlertAction> AlertActions { get; set; } = default!;
    [JsonPropertyName("priority")] public PriorityLevel Priority { get; set; } 
    [JsonPropertyName("enabled")] public bool Enabled { get; set; }

    [JsonPropertyName("last_triggered")] public DateTime? LastTriggered { get; set; }
    [JsonPropertyName("created_at")] public DateTime CreatedAt { get; set; }
    [JsonPropertyName("updated_at")] public DateTime UpdatedAt { get; set; }
}
public class Condition
{
    [JsonPropertyName("parameter")] public ParameterType Parameter { get; set; } = default!;
    [JsonPropertyName("condition")] public ConditionTypes ConditionType { get; set; } = default!;
    [JsonPropertyName("threshold")] public float? Threshold { get; set; }
    [JsonPropertyName("unit")] public string? Unit { get; set; } = default!;
    [JsonPropertyName("reasons")] public List<string>? Reasons { get; set; } = new();
}

public class AlertAction
{
    [JsonPropertyName("type")] public Types Type { get; set; } = default!; 
    [JsonPropertyName("recipients")] public List<string> Recipients { get; set; } = default!;
    [JsonPropertyName("message")] public string Message { get; set; } = default!;
}