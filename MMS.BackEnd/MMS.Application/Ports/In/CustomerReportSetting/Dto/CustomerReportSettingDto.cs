namespace MMS.Application.Ports.In.CustomerReportSetting.Dto;

public record CustomerReportSettingDto(
    Guid Id,
    string ReportName,
    string? ReportUrl,
    List<string> Email,
    List<ReportType> ReportType,
    ReportFormat Format,
    ReportFrequency Frequency,
    List<Days> WeekDays,
    DateTime ReportPeriodStartDate,
    DateTime ReportPeriodEndDate,
    List<Guid> MachineIds,
    bool IsCustomReport,
    bool IsActive,
    Guid CustomerId
);
