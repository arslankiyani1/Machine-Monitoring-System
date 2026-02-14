namespace MMS.Application.Configurations;

public class EmailSetting
{
    public string MailServer { get; set; } = default!;
    public string MailServerPort { get; set; } = default!;
    public string MailServerUserName { get; set; } = default!;
    public string MailServerPassword { get; set; } = default!;
    public bool EnableSsl { get; set; } = default!;
    public bool IsTestModel { get; set; } = default!;
    public string MailTestToAddress { get; set; } = default!;
    public string DefaultFromEmail { get; set; } = default!;
}