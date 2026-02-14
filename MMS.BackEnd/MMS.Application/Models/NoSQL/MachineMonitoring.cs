namespace MMS.Application.Models.NoSQL;

public class MachineMonitoring
{
    public string? Signals { get; set; } = default!;
    public string? Type { get; set; } = default!;
    public string? MachineName { get; set; } = default!;
    public string? Reason { get;  set; }
}

public class CreateOperationalData
{
    // public Guid MachineId { get; set; }
    public string? MachineName { get; set; } = default!;
    //public Guid CustomerId { get; set; }
    public string Type { get; set; } = default!;
    public List<OperationalDataDto> OperationalData { get; set; } = new();
}

public class OperationalDataDto
{
    [JsonPropertyName("feedrate")]
    public Measurement? FeedRate { get; set; }

    [JsonPropertyName("SpindleSpeed")]
    public Measurement? SpindleSpeed { get; set; }

    [JsonPropertyName("SpindleStatus")]
    public bool SpindleStatus { get; set; }

    [JsonPropertyName("temperature")]
    public Measurement? Temperature { get; set; }

    [JsonPropertyName("vibration")]
    public Measurement? Vibration { get; set; }

    [JsonPropertyName("PowerConsumption")]
    public Measurement? PowerConsumption { get; set; }

    [JsonPropertyName("torque")]
    public Measurement? Torque { get; set; }

    [JsonPropertyName("CoolantLevel")]
    public Measurement? CoolantLevel { get; set; }

    [JsonPropertyName("AirPressure")]
    public Measurement? AirPressure { get; set; }

    [JsonPropertyName("CycleTime")]
    public TimeSpan? CycleTime { get; set; }
}