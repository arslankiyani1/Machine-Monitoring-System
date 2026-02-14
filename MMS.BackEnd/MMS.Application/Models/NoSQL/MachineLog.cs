namespace MMS.Application.Models.NoSQL;

[Table("MachineLog")]
public class MachineLog
{
    [Key]
    [JsonPropertyName("id")] public string Id { get; set; } = default!;
    [JsonPropertyName("machine_id")] public Guid MachineId { get; set; }
    [JsonPropertyName("customer_id")] public Guid CustomerId { get; set; }
    [JsonPropertyName("user_name")] public string UserName { get; set; } = default!;
    [JsonPropertyName("user_id")] public Guid UserId { get; set; }
    [JsonPropertyName("color")] public string Color { get; set; } = default!;
    [JsonPropertyName("job_id")] public string JobId { get; set; } = default!;
    [JsonPropertyName("status")] public string Status { get; set; } = default!;
    [JsonPropertyName("type")]  public string Type { get; set; } = "MachineLog"; // default
    [JsonPropertyName("inputs")] public List<MachineInput>? Inputs { get; set; } = new();
    [JsonPropertyName("start")] public DateTime? Start { get; set; }
    [JsonPropertyName("end")] public DateTime? End { get; set; }
    [JsonPropertyName("last_update_time")] public DateTime LastUpdateTime { get; set; }
    [JsonPropertyName("comment")] public string Comment { get; set; } = default!;
    [JsonPropertyName("main_program")] public string MainProgram { get; set; } = default!;
    [JsonPropertyName("current_program")] public string CurrentProgram { get; set; } = default!;
    [JsonPropertyName("interface")] public string InterfaceName { get; set; } = "ESP32";
    [JsonPropertyName("source")] public string Source { get; set; } = default!;  // NEW FIELD
}