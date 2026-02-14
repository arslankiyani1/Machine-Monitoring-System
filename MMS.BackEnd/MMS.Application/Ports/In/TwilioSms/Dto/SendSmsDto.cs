namespace MMS.Application.Ports.In.TwilioSms.Dto;

public record SendSmsDto
{
    public string To { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string? From { get; init; }
}

public record SmsResultDto(
    string MessageSid,
    string Provider,
    string Status = "Sent",
    DateTime? SentAt = null
);

public record OfflineAlertDto
{
    public string To { get; init; } = string.Empty;
    public string MachineName { get; init; } = string.Empty;
    public string? CustomMessage { get; init; }
    public DateTime? OfflineSince { get; init; }
}