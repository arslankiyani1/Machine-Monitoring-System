public class CustomerReportSetting : Trackable
{
    public string ReportName { get; set; } = default!;
    public List<string> Email { get; set; } = new();

    public ReportFormat Format { get; set; }
    public ReportFrequency Frequency { get; set; }

    public bool IsActive { get; set; }
    public bool IsCustomReport { get; set; } = false;

    public Days[] WeekDays { get; set; } = Array.Empty<Days>();

    public DateTime ReportPeriodStartDate { get; set; }
    public DateTime ReportPeriodEndDate { get; set; }

    public Guid[] MachineIds { get; set; } = Array.Empty<Guid>();
    public ReportType[] ReportType { get; set; } = Array.Empty<ReportType>();


    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = default!;
    public CustomerReport CustomerReports { get; set; } = default!;
}
