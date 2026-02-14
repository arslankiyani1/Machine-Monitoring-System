namespace MMS.Application.Models.SQL;

public class Machine : Trackable
{
    public string MachineName { get; set; } = default!;
    public string MachineModel { get; set; } = default!;
    public string? Manufacturer { get; set; }
    public string? SerialNumber { get; set; }
    public string? Location { get; set; }
    public DateTime? InstallationDate { get; set; }
    public CommunicationProtocol CommunicationProtocol { get; set; }
    public MachineType MachineType { get; set; }
    public string? ImageUrl { get; set; } = default!;
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = default!;

    public MachineSetting MachineSetting { get; set; } = default!;
    public ICollection<UserMachine> UserMachine { get; set; } = [];
    public ICollection<MachineMaintenanceTask> MachineMaintenanceTask { get; set; } = [];
}