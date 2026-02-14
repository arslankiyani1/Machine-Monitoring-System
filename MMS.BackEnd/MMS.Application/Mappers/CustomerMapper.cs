namespace MMS.Application.Mappers;
public class CustomerMapper : Profile
{
    public CustomerMapper()
    {
        CreateMap<CustomerDto, Customer>().ReverseMap();
        CreateMap<AddCustomerDto, Customer>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ForMember(dest => dest.Shifts, opt => opt.MapFrom(src => src.Shifts))
            .ReverseMap();
        CreateMap<UpdateCustomerDto, Customer>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}