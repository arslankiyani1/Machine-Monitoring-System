namespace MMS.Application.Configurations;

public class AlertEmailSetting
{
    public List<string> DefaultRecipients { get; set; } = new();
    public string Subject { get; set; } = "Machine Alert Notification";
    public string SendWindowStart { get; set; } = "07:00";
    public string SendWindowEnd { get; set; } = "21:00";
    public bool EnableEmailAlerts { get; set; } = true;
}
