namespace MMS.Application.Mappers;

public class CustomerDashboardMapper : Profile
{
    public CustomerDashboardMapper()
    {
        CreateMap<CustomerDashboardDto, CustomerDashboard>().ReverseMap();
        CreateMap<AddCustomerDashboardDto, CustomerDashboard>()
           .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
           .ReverseMap();
        CreateMap<UpdateCustomerDashboardDto, CustomerDashboard>().ReverseMap();
    }
}