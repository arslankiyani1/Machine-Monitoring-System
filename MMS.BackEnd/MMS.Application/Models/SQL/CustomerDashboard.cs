namespace MMS.Application.Models.SQL;

public class CustomerDashboard: Trackable
{
    public string Name { get; set; } = default!;
    public int? RefreshInterval { get; set; }
    public bool IsDefault { get; set; } = false;
    public DashboardTheme Theme { get; set; } = DashboardTheme.Light;
    public DashboardStatus Status { get; set; } 
    public Dictionary<string, object> Layout { get; set; } = [];

    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = default!;
    public ICollection<Widget> Widget { get; set; } = [];
}