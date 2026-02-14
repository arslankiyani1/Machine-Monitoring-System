namespace MMS.Application.Ports.In.CustomerReport.Dto;

public record CustomerReportDto(
    Guid Id,
    Guid CustomerReportSettingId,
    Guid CustomerId,
    string ReportName,
    string BlobLink,
    ReportFormat Format,
    bool IsSent,
    DateTime GeneratedDate
    //DateTime StartDate,
    //DateTime EndTime,
    //List<string> Recipients
);