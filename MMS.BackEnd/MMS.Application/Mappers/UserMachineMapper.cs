namespace MMS.Application.Mappers;

public class UserMachineMapper : Profile
{
    public UserMachineMapper()
    {
        CreateMap<UserMachineDto, UserMachine>().ReverseMap();
        CreateMap<AddUserMachineDto, UserMachine>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ReverseMap();
        CreateMap<UpdateUserMachineDto, UserMachine>().ReverseMap();
    }
}