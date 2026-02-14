using MMS.Application.Ports.In.NoSql.MachineJob.Dto;

namespace MMS.Application.Mappers;

public class MachineJobMapper : Profile
{
    public MachineJobMapper()
    {
        {
            // 🟢 Add Mapping
            CreateMap<MachineJobAddDto, MachineJob>();
            CreateMap<MachineJobUpdateDto, MachineJob>();
            CreateMap<MachineJob, Ports.In.NoSql.MachineJob.Dto.MachineJobDto>();

            // 🔁 Sub-object mappings
            CreateMap<QuantitiesDto, Quantities>().ReverseMap();
            CreateMap<MetricsDto, Metrics>().ReverseMap();
            CreateMap<JobScheduleDto, JobSchedule>().ReverseMap();
            CreateMap<SetupPhaseDto, SetupPhase>().ReverseMap();
            CreateMap<DowntimeEventDto, DowntimeEvent>().ReverseMap();
        }
    }
}
