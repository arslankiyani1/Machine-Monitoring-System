using Microsoft.Extensions.Logging;
using MMS.Application.Ports.In.TwilioSms.Dto;

namespace MMS.Adapters.Rest.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SmsController (ISmsNotificationService _smsService) : ControllerBase
{
    /// <summary>
    /// Send an offline alert SMS for a machine
    /// </summary>
    [HttpPost("offline-alert")]
    [ProducesResponseType(typeof(ApiResponse<SmsResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SmsResultDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<SmsResultDto>), StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> SendOfflineAlert(
        [FromBody] OfflineAlertRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _smsService.SendOfflineAlertAsync(
            request.To,
            request.MachineName,
            cancellationToken);

        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Send a custom SMS message
    /// </summary>
    [HttpPost("send")]
    [ProducesResponseType(typeof(ApiResponse<SmsResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SmsResultDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendSms(
        [FromBody] SendSmsDto request,
        CancellationToken cancellationToken)
    {
        var result = await _smsService.SendCustomSmsAsync(request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}

public record OfflineAlertRequest
{
    public string To { get; init; } = string.Empty;
    public string MachineName { get; init; } = string.Empty;
}