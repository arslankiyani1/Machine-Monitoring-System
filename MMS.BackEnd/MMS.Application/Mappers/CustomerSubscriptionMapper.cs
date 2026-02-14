namespace MMS.Application.Mappers;

public class CustomerSubscriptionMapper : Profile
{
    public CustomerSubscriptionMapper()
    {
        CreateMap<CustomerSubscriptionDto, CustomerSubscription>().ReverseMap();
        CreateMap<AddCustomerSubscriptionDto, CustomerSubscription>()
             .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid())).ReverseMap();
        CreateMap<UpdateCustomerSubscriptionDto, CustomerSubscription>().ReverseMap();
    }
}