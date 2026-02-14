namespace MMS.Application.Ports.In.Support.Dto;

public record AddSupportTicketDto(
    SupportType SupportType,
    PriorityLevel PriorityLevel,
    Guid? MachineId,
    string Description,
    IFormFile? Attachment
);