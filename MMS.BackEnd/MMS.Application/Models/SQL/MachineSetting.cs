namespace MMS.Application.Models.SQL;

public class MachineSetting : Trackable
{
    public bool CycleStartInterlock { get; set; } = false;
    public bool GuestLock { get; set; } = false;
    public bool ReverseCSlockLogic { get; set; } = false;
    public bool AutomaticPartsCounter { get; set; } = false;
    public decimal? MaxFeedrate { get; set; }
    public int? MaxSpindleSpeed { get; set; }
    public TimeOnly? StopTimelimit { get; set; }
    public TimeOnly? PlannedProdusctionTime {  get; set; }
    public TimeOnly? MinElapsedCycleTime { get; set; }
    public MachineSettingsStatus Status { get; set; }

    public List<string> DownTimeReasons { get; set; } = new List<string>();   
    public Guid MachineId { get; set; }
    public Machine Machine { get; set; } = default!;
}

public class Alert
{
    public string Email { get; set; } = default!;
    public TimeOnly Time { get; set; } = default!;
}