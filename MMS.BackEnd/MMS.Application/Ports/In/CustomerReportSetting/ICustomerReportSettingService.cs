using MMS.Application.Ports.In.CustomerReportSetting.Dto;

namespace MMS.Application.Ports.In.CustomerReportSetting;

public interface ICustomerReportSettingService
{
    Task<ApiResponse<IEnumerable<CustomerReportSettingDto>>> GetListAsync(
          PageParameters pageParameters,
          ReportType? reportType,
          bool? isActive,
          ReportFrequency? reportFrequency,
          bool? isCustomReport, Guid? customerId);
    Task<ApiResponse<CustomerReportSettingDto>> GetByIdAsync(Guid id);
    //Task<ApiResponse<CustomerReportSettingDto>> AddAsync(AddCustomerReportSettingDto request);
    Task<ApiResponse<GenerateReportResponseDto>> AddAsync(AddCustomerReportSettingDto request);
    Task<ApiResponse<CustomerReportSettingDto>> UpdateAsync(Guid id, UpdateCustomerReportSettingDto request);
    Task<ApiResponse<string>> DeleteAsync(Guid id);
}

