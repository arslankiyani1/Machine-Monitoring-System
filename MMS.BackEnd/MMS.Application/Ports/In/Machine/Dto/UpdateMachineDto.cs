namespace MMS.Application.Ports.In.Machine.Dto;

public record UpdateMachineDto(
    Guid Id,
    string MachineName,
    string MachineModel,
    string? Manufacturer,
    string? SerialNumber,
    string? Location,
    DateTime? InstallationDate,
    CommunicationProtocol CommunicationProtocol,
    MachineType MachineType,
    Guid CustomerId,
    string? ImageBase64
);