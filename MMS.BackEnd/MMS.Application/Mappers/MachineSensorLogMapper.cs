using MMS.Application.Ports.In.NoSql.MachineSensorLog.Dto;
namespace MMS.Application.Mappers;

public class MachineSensorLogMapper : Profile
{
    public MachineSensorLogMapper()
    {
        // Map SensorReading to SensorReadingDto
        CreateMap<SensorReading, SensorReadingDto>();
        CreateMap<SensorReadingDto, SensorReading>();

        // Entity → DTO
        CreateMap<MachineSensorLog, MachineSensorLogDto>()
            .ForMember(dest => dest.SensorReading, opt => opt.MapFrom(src => src.SensorReading));

        // DTO → Entity
        CreateMap<AddMachineSensorLogDto, MachineSensorLog>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.SensorReading, opt => opt.MapFrom(src => src.SensorReading));

        CreateMap<UpdateMachineSensorLogDto, MachineSensorLog>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.SensorReading, opt => opt.MapFrom(src => src.SensorReading));
    }
}
