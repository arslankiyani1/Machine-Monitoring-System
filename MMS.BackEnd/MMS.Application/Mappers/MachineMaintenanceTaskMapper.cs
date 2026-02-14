namespace MMS.Application.Mappers;

public class MachineMaintenanceTaskMapper : Profile
{
    public MachineMaintenanceTaskMapper()
    {
        // ✅ Entity → DTO
        CreateMap<MachineMaintenanceTask, MachineMaintenanceTaskDto>();

        // ✅ Add DTO → Entity
        CreateMap<AddMachineMaintenanceTaskDto, MachineMaintenanceTask>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ForMember(dest => dest.Attachments, opt => opt.Ignore())
            .ForMember(dest => dest.CustomerId, opt => opt.Ignore())
            .ForMember(dest => dest.Machine, opt => opt.Ignore())
            .ForMember(dest => dest.Customer, opt => opt.Ignore());

        // ✅ Update DTO → Entity
        CreateMap<UpdateMachineMaintenanceTaskDto, MachineMaintenanceTask>()
            .ForMember(dest => dest.Attachments, opt => opt.Ignore())
            .ForMember(dest => dest.Machine, opt => opt.Ignore())
            .ForMember(dest => dest.Customer, opt => opt.Ignore());
    }
}