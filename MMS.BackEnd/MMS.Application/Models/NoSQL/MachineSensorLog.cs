namespace MMS.Application.Models.NoSQL;

[Table("MachineSensorData")]
public class MachineSensorLog
{
    [Key]
    [JsonPropertyName("id")] public Guid Id { get; set; } = default!;
    [JsonPropertyName("machine_id")] public Guid MachineId { get; set; } = default!;
    [JsonPropertyName("sensor_id")] public Guid SensorId { get; set; } = default!;
    [JsonPropertyName("sensor_name")] public string SensorName { get; set; } = default!;
    [JsonPropertyName("sensor_status")] public SensorStatus SensorStatus { get; set; } = default!;
    [JsonPropertyName("sensor_reading")] public List<SensorReading> SensorReading { get; set; } = default!;
    [JsonPropertyName("date_time")] public DateTime DateTime { get; set; }
    [JsonPropertyName("source")] public string Source { get; set; } = default!;
}

public class SensorReading
{
    [JsonPropertyName("key")] public ParameterType Key { get; set; } = default!;
    [JsonPropertyName("value")] public float Value { get; set; }
    [JsonPropertyName("unit")] public string Unit { get; set; } = default!;
}