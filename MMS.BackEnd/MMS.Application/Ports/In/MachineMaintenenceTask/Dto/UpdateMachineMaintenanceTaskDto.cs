namespace MMS.Application.Ports.In.MachineMaintenenceTask.Dto;

public record UpdateMachineMaintenanceTaskDto(
    Guid Id,
    string MaintenanceTaskName,
    string? Reason,
    string? Notes,
    bool IsFinished,
    List<IFormFile>? Files,
    List<string>? Attachments,
    MaintenanceTaskCategory Category,
    PriorityLevel Priority,
    DateTime StartTime,
    DateTime EndTime,
    DateTime ScheduledDate,
    DateTime DueDate
);

