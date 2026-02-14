namespace MMS.Application.Ports.In.UserMachine.Dto;

public record UpdateUserMachineDto(
    Guid Id,
    Guid UserId,
    Guid MachineId
);