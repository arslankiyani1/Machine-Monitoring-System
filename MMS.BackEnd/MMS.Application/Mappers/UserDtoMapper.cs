namespace MMS.Application.Mappers;

public class UserDtoMapper : Profile
{

    public UserDtoMapper()
    {
        CreateMap<SignUpUserDto, AddUserDto>()
         .ForMember(dest => dest.FirstName, opt => opt.Ignore())
         .ForMember(dest => dest.LastName, opt => opt.Ignore())
         .ForMember(dest => dest.FcmTokens, opt => opt.Ignore())
         .ForMember(dest => dest.City, opt => opt.Ignore())
         .ForMember(dest => dest.Country, opt => opt.Ignore())
         .ForMember(dest => dest.Region, opt => opt.Ignore())
         .ForMember(dest => dest.State, opt => opt.Ignore())
         .ForMember(dest => dest.TimeZone, opt => opt.Ignore())
         .ForMember(dest => dest.JobTitle, opt => opt.Ignore())
         .ForMember(dest => dest.Department, opt => opt.Ignore())
         .ForMember(dest => dest.Role, opt => opt.Ignore());
    }
}
