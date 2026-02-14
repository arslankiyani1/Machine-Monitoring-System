namespace MMS.Application.Mappers;

public class MachineSettingMapper : Profile
{
    public MachineSettingMapper()
    {
        CreateMap<MachineSettingDto, MachineSetting>().ReverseMap();
        CreateMap<AddMachineSettingDto, MachineSetting>()
             .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid())).ReverseMap();
        CreateMap<UpdateMachineSettingDto, MachineSetting>().ReverseMap();
    }
}