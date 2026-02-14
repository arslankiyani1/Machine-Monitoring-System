namespace MMS.Application.Models.SQL;

public class Widget : Trackable
{
    public WidgetType WidgetType { get; set; }
    public WidgetSourceType SourceType { get; set; }
    public Dictionary<string, object> Config { get; set; } = [];
    public Guid DashboardId { get; set; }
    public CustomerDashboard CustomerDashboard { get; set; } = default!;
}