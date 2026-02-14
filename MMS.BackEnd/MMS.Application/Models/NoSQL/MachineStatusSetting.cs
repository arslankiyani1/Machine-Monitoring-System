namespace MMS.Application.Models.NoSQL;

[Table("MachineStatusSetting")]
public class MachineStatusSetting
{
    [Key]
    [JsonPropertyName("id")] public string Id { get; set; } = default!;
    [JsonPropertyName("machine_id")] public Guid MachineId { get; set; }
    [JsonPropertyName("inputs")] public List<MachineInput>? Inputs { get; set; } = new();
}

public class MachineInput
{
    [JsonPropertyName("input_key")] public string? InputKey { get; set; } // to track input name (e.g., input_one)
    [JsonPropertyName("signals")] public string? Signals { get; set; }
    [JsonPropertyName("color")] public string? Color { get; set; }
    [JsonPropertyName("status")] public string? Status { get; set; }
}