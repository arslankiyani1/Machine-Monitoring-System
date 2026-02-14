namespace MMS.Application.Ports.In.UserMachine.Dto;

public record UserMachineDto(
    Guid Id,
    Guid MachineId,
    Guid UserId
);