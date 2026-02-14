namespace MMS.Application.Mappers;

public class MachineMapper : Profile
{
    public MachineMapper()
    {
        CreateMap<MachineDto, Machine>().ReverseMap();
        CreateMap<AddMachineDto, Machine>()
             .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid())).ReverseMap();
        CreateMap<UpdateMachineDto, Machine>().ReverseMap();
    }
}