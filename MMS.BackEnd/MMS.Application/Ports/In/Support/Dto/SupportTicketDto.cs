namespace MMS.Application.Ports.In.Support.Dto;

public record SupportTicketDto(
    Guid Id,
    SupportType SupportType,
    PriorityLevel PriorityLevel,
    SupportStatus Status,
    Guid? MachineId,
    string? MachineName,
    string? MachineSerialNumber,
    Guid CustomerId,
    string? CustomerName,
    Guid UserId,
    string UserName,
    string Description,
    string? Urls
);