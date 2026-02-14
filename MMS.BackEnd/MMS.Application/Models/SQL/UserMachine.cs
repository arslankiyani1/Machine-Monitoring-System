namespace MMS.Application.Models.SQL;

public class UserMachine : Trackable
{
    public Guid UserId { get; set; }
    //public User User { get; set; } = default!;
    
    public Guid MachineId { get; set; }
    public Machine Machine { get; set; } = default!;
}