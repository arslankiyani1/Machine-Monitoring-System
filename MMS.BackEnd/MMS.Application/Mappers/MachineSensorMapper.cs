namespace MMS.Application.Mappers;

public class MachineSensorMapper : Profile
{
    public MachineSensorMapper()
    {
        // DTO <-> Entity
        CreateMap<MachineSensor, MachineSensorDto>().ReverseMap();

        CreateMap<AddMachineSensorDto, MachineSensor>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()));

        CreateMap<UpdateMachineSensorDto, MachineSensor>()
            .ForAllMembers(opts =>
                opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}
