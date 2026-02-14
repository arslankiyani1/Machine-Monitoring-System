using MMS.Application.Ports.In.NoSql.MachineLog.Dto;

namespace MMS.Application.Mappers;

public class MachineLogMapper : Profile
{
    public MachineLogMapper()
    {
        CreateMap<MachineLog, MachineLog>()
           .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

           CreateMap<MachineLog, MachineLogSignalRDto>()
            .ReverseMap()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}