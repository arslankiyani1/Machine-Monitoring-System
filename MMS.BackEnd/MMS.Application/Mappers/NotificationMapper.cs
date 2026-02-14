namespace MMS.Application.Mappers;

public class NotificationMapper : Profile
{
    public NotificationMapper()
    {
        CreateMap<NotificationDto, Notification>().ReverseMap();
        CreateMap<AddNotificationDto, Notification>()
             .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
             .ForMember(dest => dest.ReadStatus, opt => opt.MapFrom(_ => NotificationStatus.Unread))
             .ReverseMap();
        CreateMap<UpdateNotificationDto, Notification>().ReverseMap();
    }
}