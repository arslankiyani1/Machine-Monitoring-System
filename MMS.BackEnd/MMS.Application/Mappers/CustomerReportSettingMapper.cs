using MMS.Application.Ports.In.CustomerReportSetting.Dto;

namespace MMS.Application.Mappers;

public class CustomerReportSettingMapper : Profile
{
    public CustomerReportSettingMapper()
    {
        // Main DTO
        CreateMap<CustomerReportSettingDto, CustomerReportSetting>().ReverseMap();

        // Add DTO → Domain
        CreateMap<AddCustomerReportSettingDto, CustomerReportSetting>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        // Update DTO → Domain
        CreateMap<UpdateCustomerReportSettingDto, CustomerReportSetting>()
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        // Reverse mapping also works for Add/Update if needed
        CreateMap<CustomerReportSetting, AddCustomerReportSettingDto>().ReverseMap();

        CreateMap<CustomerReportSetting, CustomerReportSettingDto>()
            .ForMember(dest => dest.ReportUrl, opt => opt.MapFrom(a => a.CustomerReports.BlobLink));

        CreateMap<CustomerReportSetting, CustomerReportSettingDto>()
           .ConstructUsing(src => new CustomerReportSettingDto(
               src.Id,
               src.ReportName,
               src.CustomerReports != null
                   ? src.CustomerReports.BlobLink
                   : null,
               src.Email,
               src.ReportType.ToList(),
               src.Format,
               src.Frequency,
               src.WeekDays.ToList(),
               src.ReportPeriodStartDate,
               src.ReportPeriodEndDate,
               src.MachineIds.ToList(),
               src.IsCustomReport,
               src.CustomerReports != null && !string.IsNullOrEmpty(src.CustomerReports.BlobLink)
                                           && !string.IsNullOrWhiteSpace(src.CustomerReports.BlobLink),
               src.CustomerId
           ));

        CreateMap<CustomerReportSetting, UpdateCustomerReportSettingDto>().ReverseMap();
    }
}
