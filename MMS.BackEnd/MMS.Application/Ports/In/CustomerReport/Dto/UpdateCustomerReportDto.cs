namespace MMS.Application.Ports.In.CustomerReport.Dto;


public record UpdateCustomerReportDto(
    Guid Id,
    Guid CustomerReportSettingId,
    Guid CustomerId,
    string ReportName,
    string BlobLink,
    ReportFormat Format,
    bool IsSent,
    DateTime GeneratedDate
);