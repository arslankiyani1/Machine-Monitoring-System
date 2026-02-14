namespace MMS.Adapters.Email;

public class EmailService(
    IOptions<EmailSetting> emailSetting,
    IEmailQueueService emailQueueService,
    IConfiguration configuration) : IEmailService
{
    #region Public Methods

    public async Task SendAccountActivatedEmailAsync(string toEmail, string userName)
    {
        var messageBody = await GetAccountActivatedEmailTemplateAsync(userName);
        emailQueueService.QueueEmail(() => SendAsync(toEmail, EmailConstants.AccountActivatedSubject, messageBody, emailSetting.Value));
    }
    public async Task SendConfirmationEmailAsync(string toEmail, string userName, string token)
    {
        var messageBody = await GetConfirmationEmailTemplateAsync(toEmail, userName, token);
        emailQueueService.QueueEmail(() => SendAsync(toEmail, EmailConstants.ConfirmationEmailSubject, messageBody, emailSetting.Value));
    }
    public async Task SendContactUsEmailAsync(SupportEmailDto emailDto)
    {
        var messageBody = await GetContactUsEmailTemplateAsync(emailDto);
        var contactUsEmails = configuration["Emails:ContactUsEmails"];
        emailQueueService.QueueEmail(() => SendAsync(contactUsEmails!, emailDto.Subject!, messageBody, emailSetting.Value));
    }
    public async Task SendResetPasswordEmailAsync(string email, string username, string token, RequestedPortal requestedPortal)
    {
        var messageBody = await GetResetPasswordEmailTemplateAsync(email, username, token, requestedPortal);
        emailQueueService.QueueEmail(() => SendAsync(email, EmailConstants.ResetPasswordSubject, messageBody, emailSetting.Value));
    }
    public async Task SendMachineAlertEmailAsync(string toEmail, string machineName, string alertMessage, string alertTime)
    {
        var messageBody = await GetMachineAlertEmailTemplateAsync(machineName, alertMessage, alertTime);
        emailQueueService.QueueEmail(() => SendAsync(toEmail, "Machine Alert Notification", messageBody, emailSetting.Value));
    }

    public async Task SendSupportTicketCreatedEmailAsync(string toEmail, string userName, Guid supportId, Guid customerId)
    {
        var messageBody = await GetSupportTicketCreatedEmailTemplateAsync(userName, supportId, customerId);
        emailQueueService.QueueEmail(() => SendAsync(toEmail, "New Support Ticket Created", messageBody, emailSetting.Value));
    }

    public async Task<bool> SendReportAsync(List<string> toEmails, string reportName, byte[] fileBytes, string format)
    {
        var messageBody = await GetReportEmailTemplateAsync(reportName);
        var fileExtension = format.ToLower() switch
        {
            "pdf" => ".pdf",
            "csv" => ".csv",
            "excel" => ".xlsx",
            _ => ".dat"
        };
        var fileName = $"{reportName}{fileExtension}";
        var attachments = new List<(byte[] FileContent, string FileName)> { (fileBytes, fileName) };
        var subject = $"Your MMS Report: {reportName}";
        
        foreach (var email in toEmails)
        {
            await SendAsync(email, subject, messageBody, emailSetting.Value, attachments);
        }
        return true;
    }
    public async Task<bool> SendAsync(string toEmail, string subject, string message,
        EmailSetting emailSetting, List<(byte[] FileContent, string FileName)>? attachments = null)
    {
        var mail = new MailMessage
        {
            Subject = subject,
            Body = message,
            IsBodyHtml = true,
            From = new MailAddress(emailSetting.DefaultFromEmail)
        };
        mail.To.Add(toEmail);
        if (attachments != null)
        {
            foreach (var (fileContent, fileName) in attachments)
            {
                if (fileContent?.Length > 0)
                {
                    var ms = new MemoryStream(fileContent);
                    mail.Attachments.Add(new Attachment(ms, fileName));
                }
            }
        }
        using var smtpClient = new SmtpClient
        {
            Host = emailSetting.MailServer,
            Port = int.TryParse(emailSetting.MailServerPort, out var port) ? port : 25,
            UseDefaultCredentials = false,
            EnableSsl = true,
            Credentials = new NetworkCredential(emailSetting.MailServerUserName, emailSetting.MailServerPassword)
        };
        try
        {
            await smtpClient.SendMailAsync(mail);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending email: {ex.Message}");
            return false;
        }
        finally
        {
            foreach (var attachment in mail.Attachments) attachment.Dispose();
        }
    }

    #endregion

    #region Private Methods

    private async Task<string> GetAccountActivatedEmailTemplateAsync(string userName)
    {
        var loginUrl = configuration?.GetValue<string>("ApplicationUrls:LoginUrl");
        var templatePath = Path.Combine(AppContext.BaseDirectory, ApplicationConstants.AccountActivatedEmailTemplatePath);
        var template = await System.IO.File.ReadAllTextAsync(templatePath);
        return template
            .Replace("{userName}", userName)
            .Replace("{loginRedirect}", loginUrl);
    }
    private async Task<string> GetConfirmationEmailTemplateAsync(string userEmail, string userName, string token)
    {
        var queryParams = HttpUtility.ParseQueryString(string.Empty);
        queryParams["token"] = token;
        queryParams["email"] = userEmail;
        var callbackUrl = configuration?.GetValue<string>("ApplicationUrls:ConfirmEmailUrl");
        var url = new UriBuilder(callbackUrl!)
        {
            Query = queryParams.ToString()
        }.ToString();
        var templatePath = Path.Combine(AppContext.BaseDirectory, ApplicationConstants.ConfirmationEmailTemplatePath);
        var template = await File.ReadAllTextAsync(templatePath);
        return template
            .Replace("{userName}", userName)
            .Replace("{emailConfirmationUrl}", url);
    }
    private async Task<string> GetResetPasswordEmailTemplateAsync(string email, string username, string token, RequestedPortal requestedPortal)
    {
        var queryParams = HttpUtility.ParseQueryString(string.Empty);
        queryParams["code"] = token;
        queryParams["email"] = email;
        var callbackUrl = requestedPortal == RequestedPortal.UserPortal ? configuration?.GetValue<string>("ApplicationUrls:ResetPasswordUrl") : configuration?.GetValue<string>("ApplicationUrls:CPResetPasswordUrl");
        var resetPasswordUrl = new UriBuilder(callbackUrl!)
        {
            Query = queryParams.ToString()
        }.ToString();
        var templatePath = Path.Combine(AppContext.BaseDirectory, ApplicationConstants.ResetPasswordEmailTemplatePath);
        var template = await File.ReadAllTextAsync(templatePath);
        return template
            .Replace("{username}", username)
            .Replace("{resetPasswordRedirect}", resetPasswordUrl);
    }
    private async Task<string> GetContactUsEmailTemplateAsync(SupportEmailDto emailDto)
    {
        var templatePath = Path.Combine(AppContext.BaseDirectory, ApplicationConstants.ContactUsEmailTemplatePath);
        var template = await File.ReadAllTextAsync(templatePath);
        return template.Replace("{Name}", emailDto.Name)
            .Replace("{Email}", emailDto.Email)
            .Replace("{Message}", emailDto.Message)
            .Replace("{Year}", DateTime.Today.Year.ToString());
    }

    private async Task<string> GetMachineAlertEmailTemplateAsync(string machineName, string alertMessage, string alertTime)
    {
        var templatePath = Path.Combine(AppContext.BaseDirectory, ApplicationConstants.MachineAlertEmailTemplatePath);
        var template = await File.ReadAllTextAsync(templatePath);
        return template
            .Replace("{machineName}", machineName)
            .Replace("{alertMessage}", alertMessage)
            .Replace("{alertTime}", alertTime.ToString())
            .Replace("{Year}", DateTime.Today.Year.ToString());
    }

    private async Task<string> GetReportEmailTemplateAsync(string reportName)
    {
        var templatePath = Path.Combine(AppContext.BaseDirectory, ApplicationConstants.ReportEmailTemplatePath);
        var template = await System.IO.File.ReadAllTextAsync(templatePath);
        return template
            .Replace("{reportName}", reportName)
            .Replace("{Year}", DateTime.Today.Year.ToString());
    }

    private async Task<string> GetSupportTicketCreatedEmailTemplateAsync(string userName, Guid supportId, Guid customerId)
    {
        var templatePath = Path.Combine(AppContext.BaseDirectory, ApplicationConstants.SupportTicketCreatedEmailTemplatePath);
        var template = await File.ReadAllTextAsync(templatePath);

        return template
            .Replace("{userName}", userName)
            .Replace("{supportId}", supportId.ToString())
            .Replace("{customerId}", customerId.ToString())
            .Replace("{Year}", DateTime.Today.Year.ToString());
    }

    #endregion
}
