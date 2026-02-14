namespace MMS.Application.Models.NoSQL;

[Table("OperationalData")]
public class OperationalData
{
    [Key]
    [JsonPropertyName("id")] public string Id { get; set; } = default!;
    [JsonPropertyName("machine_id")] public Guid MachineId { get; set; }
    [JsonPropertyName("customer_id")] public Guid CustomerId { get; set; }
    [JsonPropertyName("timestamp")] public DateTime Timestamp { get; set; }
    [JsonPropertyName("type")] public string Type { get; set; } = default!;
    [JsonPropertyName("measurement")] public Measurement Measurement { get; set; } = default!;
    [JsonPropertyName("source")] public string Source { get; set; } = default!; 
}

public class Measurement
{
    [JsonPropertyName("value")]
    public float Value { get; set; }

    [JsonPropertyName("unit")]
    public string Unit { get; set; } = default!;
}