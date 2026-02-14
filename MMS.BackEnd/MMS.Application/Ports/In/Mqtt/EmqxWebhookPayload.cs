namespace MMS.Application.Ports.In.Mqtt;

// Payload structure sent by EMQX Cloud webhook
public class EmqxWebhookPayload
{
    [JsonPropertyName("topic")]  public string Topic { get; set; } = default!;
    [JsonPropertyName("payload")] public MachineMonitoring Payload { get; set; } = default!;
    [JsonPropertyName("username")]  public string Username { get; set; } = default!;
    [JsonPropertyName("timestamp")] public long Timestamp { get; set; }
}