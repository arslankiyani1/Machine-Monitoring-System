namespace MMS.Application.Interfaces;

public interface IEmailService
{
    Task SendAccountActivatedEmailAsync(string toEmail, string userName);
    Task SendResetPasswordEmailAsync(string email, string username, string token, RequestedPortal requestedPortal);
    Task SendConfirmationEmailAsync(string toEmail, string userName, string token);
    Task SendContactUsEmailAsync(SupportEmailDto emailDto);
    Task<bool> SendReportAsync(List<string> toEmails, string reportName, byte[] fileBytes, string format);
    Task SendMachineAlertEmailAsync(string toEmail, string machineName, string alertMessage, string alertTime);
    // ✅ New method for support ticket creation
    Task SendSupportTicketCreatedEmailAsync(string toEmail, string userName, Guid supportId, Guid customerId);
}