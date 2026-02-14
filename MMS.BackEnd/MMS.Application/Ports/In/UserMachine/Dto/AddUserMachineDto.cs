namespace MMS.Application.Ports.In.UserMachine.Dto;

public record AddUserMachineDto(
    Guid UserId,
    Guid MachineId
);
