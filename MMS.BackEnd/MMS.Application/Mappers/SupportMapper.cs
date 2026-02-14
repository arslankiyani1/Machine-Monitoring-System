namespace MMS.Application.Mappers;

public class SupportMapper : Profile
{
    public SupportMapper()
    {
        CreateMap<SupportTicketDto, Support>().ReverseMap();

        CreateMap<AddSupportTicketDto, Support>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ReverseMap();

        CreateMap<UpdateSupportTicketDto, Support>().ReverseMap();
    }
}