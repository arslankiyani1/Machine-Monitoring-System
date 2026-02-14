namespace MMS.Application.Common.Constants;

public class ApplicationConstants
{
    public const string ApplicationName = "MMS";
    public const string ApplicationVersion = "1.0.0.0";
    public static readonly string AccountActivatedEmailTemplatePath = Path.Combine("EmailTemplates", "account_activated.html");
    public static readonly string ConfirmationEmailTemplatePath = Path.Combine("EmailTemplates", "confirm_email.html");
    public static readonly string ResetPasswordEmailTemplatePath = Path.Combine("EmailTemplates", "reset_password.html");
    public static readonly string ContactUsEmailTemplatePath = Path.Combine("EmailTemplates", "contact_us_email.html");
    public static readonly string MachineAlertEmailTemplatePath = Path.Combine("EmailTemplates", "MachineAlert.html");
    public static readonly string ReportEmailTemplatePath = Path.Combine("EmailTemplates", "ReportEmailTemplate.html");
    public static readonly string SupportTicketCreatedEmailTemplatePath = Path.Combine("EmailTemplates", "SupportTicketCreated.html");
}