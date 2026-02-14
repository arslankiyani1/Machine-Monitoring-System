using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MMS.Application.Common;
using MMS.Application.Interfaces;
using MMS.Application.Ports.In.TwilioSms.Dto;
using AdapterTwilio.Abstractions;

namespace AdapterTwilio.Services;

/// <summary>
/// SMS notification service following SOLID principles
/// - Single Responsibility: Validates and delegates to port
/// - Dependency Inversion: Depends on ISmsNotificationPort abstraction
/// </summary>
public class SmsNotificationService : ISmsNotificationService
{
    private readonly ISmsNotificationPort _smsPort;
    private readonly ILogger<SmsNotificationService> _logger;
    private readonly IPhoneNumberNormalizer _phoneNormalizer;

    public SmsNotificationService(
        ISmsNotificationPort smsPort,
        IPhoneNumberNormalizer phoneNormalizer,
        ILogger<SmsNotificationService> logger)
    {
        _smsPort = smsPort ?? throw new ArgumentNullException(nameof(smsPort));
        _phoneNormalizer = phoneNormalizer ?? throw new ArgumentNullException(nameof(phoneNormalizer));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ApiResponse<SmsResultDto>> SendOfflineAlertAsync(
        string to,
        string machineName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(to))
            return new ApiResponse<SmsResultDto>(StatusCodes.Status400BadRequest, "Phone number is required");

        if (string.IsNullOrWhiteSpace(machineName))
            return new ApiResponse<SmsResultDto>(StatusCodes.Status400BadRequest, "Machine name is required");

        _logger.LogInformation("Sending offline alert for machine {MachineName} to {To}", 
            machineName, _phoneNormalizer.Mask(to));

        var dto = new OfflineAlertDto
        {
            To = to,
            MachineName = machineName,
            OfflineSince = DateTime.UtcNow
        };

        return await _smsPort.SendOfflineAlertAsync(dto, cancellationToken);
    }

    public async Task<ApiResponse<SmsResultDto>> SendCustomSmsAsync(
        SendSmsDto dto,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.To))
            return new ApiResponse<SmsResultDto>(StatusCodes.Status400BadRequest, "Phone number is required");

        if (string.IsNullOrWhiteSpace(dto.Message))
            return new ApiResponse<SmsResultDto>(StatusCodes.Status400BadRequest, "Message is required");

        if (dto.Message.Length > 1600)
            return new ApiResponse<SmsResultDto>(StatusCodes.Status400BadRequest, "Message exceeds 1600 characters");

        return await _smsPort.SendAsync(dto, cancellationToken);
    }
}
