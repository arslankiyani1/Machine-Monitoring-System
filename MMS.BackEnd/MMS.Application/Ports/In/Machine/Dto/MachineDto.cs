namespace MMS.Application.Ports.In.Machine.Dto;

public record MachineDto(
    Guid Id,
    string MachineName,
    string MachineModel,
    string? Manufacturer,
    string? SerialNumber,
    string? Location,
    DateTime? InstallationDate,
    CommunicationProtocol CommunicationProtocol,
    MachineType MachineType,
    Guid? CustomerId,
    string? ImageUrl
);

public record CustomerWithMachinesDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = default!;
    public string? ImageUrl { get; init; }
    public List<MachineDto> Machines { get; init; } = new();
}