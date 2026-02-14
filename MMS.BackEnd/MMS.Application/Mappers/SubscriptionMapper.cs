namespace MMS.Application.Mappers;

public class SubscriptionMapper : Profile
{
    public SubscriptionMapper()
    {
        CreateMap<SubscriptionDto, Subscription>().ReverseMap();

        CreateMap<SubscriptionAddDto, Subscription>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ReverseMap();

        CreateMap<SubscriptionUpdateDto, Subscription>().ReverseMap();
    }
}