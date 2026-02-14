namespace MMS.Application.Models.NoSQL;

[Table("Alerts")]
public class Alert
{
    [Key]
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("alert_id")]
    public Guid AlertId { get; set; } = default!;

    [JsonPropertyName("customer_id")]
    public Guid CustomerId { get; set; }

    [JsonPropertyName("machine_id")]
    public Guid MachineId { get; set; }

    [JsonPropertyName("rule_name")]
    public string RuleName { get; set; } = default!;

    [JsonPropertyName("status")]
    public string Status { get; set; } = default!;

    [JsonPropertyName("message")]
    public string Message { get; set; } = default!;

    [JsonPropertyName("operational_data")]
    public string OperationalData { get; set; } = default!;

    [JsonPropertyName("triggered_at")]
    public DateTime TriggeredAt { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
}