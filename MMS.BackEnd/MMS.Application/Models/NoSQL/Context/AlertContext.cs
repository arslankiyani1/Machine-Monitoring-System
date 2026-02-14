namespace MMS.Application.Models.NoSQL.Context;

public record AlertContext
{
    public required Guid MachineId { get; init; }
    public required Guid CustomerId { get; init; }
    public required string MachineName { get; init; }
    public required string Status { get; init; }
    public required string OperationalData { get; init; }
    public required string Priority { get; init; }
    public required DateTime? TriggeredAt { get; init; }
    public string? Source { get; init; }
    public string? JobId { get; init; }
    public string? UserName { get; init; }
}
