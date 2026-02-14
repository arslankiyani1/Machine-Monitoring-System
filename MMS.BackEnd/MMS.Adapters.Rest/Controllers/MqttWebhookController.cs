using MMS.Application.Ports.In.Mqtt;

namespace MMS.Adapters.Rest.Controllers;

[ApiController]
[Route("api/mqtt")]
public class MqttWebhookController(
    IMachineMonitoringService monitoring,
    ILogger<MqttWebhookController> logger) : BaseController
{
    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> HandleMqttMessage([FromBody] EmqxWebhookPayload webhookPayload)
    {
        try
        {
            logger.LogInformation(
                "Received MQTT webhook from device '{Username}' on topic '{Topic}'",
                webhookPayload.Username, webhookPayload.Topic
            );

            if (webhookPayload.Payload == null)
            {
                logger.LogWarning(
                    "Invalid payload format from device '{Username}': {Payload}",
                    webhookPayload.Username, webhookPayload.Payload
                );
                return BadRequest(new { error = "Invalid payload format" });
            }

            // Process through existing MachineMonitoringService
            await monitoring.ProcessMonitoringAsync(webhookPayload.Payload);

            logger.LogInformation(
                "Successfully processed MQTT message from device '{Username}' on topic '{Topic}'",
                webhookPayload.Username, webhookPayload.Topic
            );

            return Ok(new
            {
                status = "success",
                processed = true,
                timestamp = DateTime.UtcNow
            });
        }
        catch (JsonException ex)
        {
            logger.LogError(ex,
                "Failed to deserialize MQTT payload: {Payload}",
                JsonSerializer.Serialize(webhookPayload?.Payload)
            );
            return BadRequest(new { error = "Invalid JSON format", details = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error processing MQTT webhook from device '{Username}'",
                webhookPayload?.Username ?? "unknown"
            );
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}