namespace MMS.Application.Mappers;

public class WidgetMapper : Profile
{
    public WidgetMapper()
    {
        CreateMap<WidgetDto, Widget>().ReverseMap();
        CreateMap<AddWidgetDto, Widget>()
             .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid())).ReverseMap();
        CreateMap<UpdateWidgetDto, Widget>().ReverseMap();
    }
}