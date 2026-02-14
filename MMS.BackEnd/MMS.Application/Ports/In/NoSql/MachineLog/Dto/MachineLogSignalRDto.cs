namespace MMS.Application.Ports.In.NoSql.MachineLog.Dto;

public record MachineLogSignalRDto(
    string Id,
    Guid MachineId,
    Guid CustomerId,
    string Name,
    Guid UserId,
    string Color,
    string Status,
    string? JobId,
    string? UserName,
    Dictionary<string, int> StatusSummary
);