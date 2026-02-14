namespace MMS.Application.Models.SQL;

public class CustomerReport : Trackable
{
    public Guid CustomerReportSettingId { get; set; }
    public Guid CustomerId { get; set; }

    public string ReportName { get; set; } = default!;
    public string? BlobLink { get; set; } = default!;
    public ReportFormat Format { get; set; }
    public bool IsSent { get; set; }
    public DateTime GeneratedDate { get; set; }

    // Navigation Property
    public CustomerReportSetting CustomerReportSetting { get; set; }=default!;
    public Customer Customer { get; set; } = default!;
}
