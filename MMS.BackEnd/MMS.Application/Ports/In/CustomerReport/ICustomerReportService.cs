namespace MMS.Application.Ports.In.CustomerReport;
public interface ICustomerReportService
{
    Task<ApiResponse<CustomerReportDto>> AddAsync(AddCustomerReportDto request);

    Task<ApiResponse<string>> DeleteAsync(Guid id);

    Task<ApiResponse<CustomerReportDto>> GetByIdAsync(Guid id);

    Task<ApiResponse<IEnumerable<CustomerReportDto>>> GetListAsync(
        PageParameters pageParameters,
        ReportType? reportType,
        bool? isSent,
        ReportFrequency? reportFrequency,
        Guid? customerId
    );
}