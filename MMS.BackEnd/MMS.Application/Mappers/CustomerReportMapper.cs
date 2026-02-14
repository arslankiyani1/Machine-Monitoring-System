namespace MMS.Application.Mappers;


public class CustomerReportMapperProfile : Profile
{
    public CustomerReportMapperProfile()
    {
        // Entity -> DTO (record)
        CreateMap<CustomerReport, CustomerReportDto>();

        // Create DTO -> Entity
        CreateMap<AddCustomerReportDto, CustomerReport>();

        // Update DTO -> Entity
        CreateMap<UpdateCustomerReportDto, CustomerReport>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()); // Usually ignore ID
    }
}
