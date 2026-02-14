namespace MMS.Application.Ports.In.MachineMaintenenceTask.Dto;

public record MachineMaintenanceTaskDto(
    Guid Id,
    string MaintenanceTaskName,
    string? Reason,
    string? Notes,
    bool IsFinished,
    List<string> Attachments, 
    MaintenanceTaskCategory Category,
    PriorityLevel Priority,
    DateTime StartTime,
    DateTime EndTime,
    DateTime ScheduledDate,
    DateTime DueDate,
    Guid? AssignedToUserId,
    string? AssignedToUserName,
    Guid? JobId,
    Guid MachineId,
    Guid CustomerId
);