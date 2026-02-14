namespace MMS.Application.Models.SQL;

public class Support : Trackable
{
    public SupportType SupportType { get; set; } = default!;
    public PriorityLevel PriorityLevel { get; set; } = default!;
    public SupportStatus Status { get; set; } = default!;
    public Guid? MachineId { get; set; } = default!;
    public Guid CustomerId { get; set; } = default!;
    public string? CustomerName { get; set; } = default!;
    public string? MachineName { get; set; }
    public string? MachineSerialNumber { get; set; }
    public string UserName { get; set; } = default!;
    public Guid UserId { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string? Urls { get; set; }
}
