namespace MMS.Application.Ports.In.Support.Dto;

public record UpdateSupportTicketDto(
     SupportType SupportType,
     PriorityLevel PriorityLevel,
     Guid? MachineId,
     string Description,
     IFormFile? Attachment
 );