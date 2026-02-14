namespace MMS.Application.Ports.In.CustomerReportSetting.Dto;

public record AddCustomerReportSettingDto(
    string ReportName,
    List<string> Email,
    List<ReportType> ReportType,
    ReportFormat Format,
    ReportFrequency Frequency,
    DateTime? ReportPeriodStartDate,
    DateTime? ReportPeriodEndDate,
    List<Guid> MachineIds,
    bool IsCustomReport,
    Guid CustomerId
);
