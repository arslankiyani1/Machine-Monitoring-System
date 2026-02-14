using MMS.Application.Ports.In.TwilioSms.Dto;

namespace MMS.Application.Interfaces;

public interface ISmsNotificationPort
{
    Task<ApiResponse<SmsResultDto>> SendAsync(SendSmsDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<SmsResultDto>> SendOfflineAlertAsync(OfflineAlertDto dto, CancellationToken cancellationToken = default);
    Task<bool> ValidatePhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default);
}

public interface ISmsNotificationService
{
    Task<ApiResponse<SmsResultDto>> SendOfflineAlertAsync(string to, string machineName, CancellationToken cancellationToken = default);
    Task<ApiResponse<SmsResultDto>> SendCustomSmsAsync(SendSmsDto dto, CancellationToken cancellationToken = default);
}
