namespace MMS.Application.Ports.In.CustomerReport.Dto;

public record AddCustomerReportDto(
    Guid CustomerReportSettingId,
    Guid CustomerId,
    string ReportName,
    string BlobLink,
    ReportFormat Format,
    bool IsSent,
    DateTime GeneratedDate
);
