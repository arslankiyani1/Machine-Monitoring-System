using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MMS.Application.Common;
using MMS.Application.Interfaces;
using MMS.Application.Ports.In.TwilioSms.Dto;
using Polly.Retry;
using Twilio.Exceptions;
using Twilio.Types;
using AdapterTwilio.Abstractions;

namespace AdapterTwilio.Services;

/// <summary>
/// Twilio SMS adapter following SOLID principles
/// - Single Responsibility: Orchestrates SMS sending using specialized services
/// - Open/Closed: Extensible through interfaces
/// - Liskov Substitution: Properly implements ISmsNotificationPort
/// - Interface Segregation: Uses focused interfaces
/// - Dependency Inversion: Depends on abstractions
/// </summary>
public class TwilioSmsAdapter : ISmsNotificationPort
{
    private readonly TwilioSettings _settings;
    private readonly ILogger<TwilioSmsAdapter> _logger;
    private readonly ITwilioClientWrapper _twilioClient;
    private readonly IPhoneNumberNormalizer _phoneNormalizer;
    private readonly ITwilioErrorMapper _errorMapper;
    private readonly IMessageTemplateService _templateService;
    private readonly AsyncRetryPolicy _retryPolicy;

    public TwilioSmsAdapter(
        ITwilioClientWrapper twilioClient,
        IPhoneNumberNormalizer phoneNormalizer,
        ITwilioErrorMapper errorMapper,
        IMessageTemplateService templateService,
        IRetryPolicyFactory retryPolicyFactory,
        IOptions<TwilioSettings> settings,
        ILogger<TwilioSmsAdapter> logger)
    {
        _twilioClient = twilioClient ?? throw new ArgumentNullException(nameof(twilioClient));
        _phoneNormalizer = phoneNormalizer ?? throw new ArgumentNullException(nameof(phoneNormalizer));
        _errorMapper = errorMapper ?? throw new ArgumentNullException(nameof(errorMapper));
        _templateService = templateService ?? throw new ArgumentNullException(nameof(templateService));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _settings.Validate();
        _twilioClient.Initialize(_settings.AccountSid, _settings.AuthToken);
        _retryPolicy = retryPolicyFactory.CreateRetryPolicy();
    }

    public async Task<ApiResponse<SmsResultDto>> SendAsync(
        SendSmsDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var normalizedTo = _phoneNormalizer.Normalize(dto.To);
            var from = string.IsNullOrWhiteSpace(dto.From)
                ? _settings.FromNumber
                : dto.From;

            _logger.LogDebug("Sending SMS to {To}", _phoneNormalizer.Mask(normalizedTo));

            var message = await _retryPolicy.ExecuteAsync(async () =>
                await _twilioClient.SendMessageAsync(
                    dto.Message,
                    new PhoneNumber(from),
                    new PhoneNumber(normalizedTo)
                ));

            _logger.LogInformation(
                "SMS sent successfully. SID: {Sid}, Status: {Status}",
                message.Sid, message.Status);

            return new ApiResponse<SmsResultDto>(
                StatusCodes.Status200OK,
                "SMS sent successfully",
                new SmsResultDto(
                    message.Sid,
                    "Twilio",
                    message.Status.ToString(),
                    DateTime.UtcNow));
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, "Twilio API error. Code: {Code}", ex.Code);
            return new ApiResponse<SmsResultDto>(
                StatusCodes.Status502BadGateway,
                _errorMapper.MapError(ex));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error sending SMS");
            return new ApiResponse<SmsResultDto>(
                StatusCodes.Status500InternalServerError,
                "Failed to send SMS. Please try again later.");
        }
    }

    public async Task<ApiResponse<SmsResultDto>> SendOfflineAlertAsync(
        OfflineAlertDto dto,
        CancellationToken cancellationToken = default)
    {
        var message = _templateService.ProcessOfflineAlertTemplate(
            _settings.OfflineAlertTemplate,
            dto.MachineName,
            dto.OfflineSince ?? DateTime.UtcNow);

        return await SendAsync(new SendSmsDto
        {
            To = dto.To,
            Message = message
        }, cancellationToken);
    }

    public Task<bool> ValidatePhoneNumberAsync(
        string phoneNumber,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_phoneNormalizer.IsValid(phoneNumber));
    }
}