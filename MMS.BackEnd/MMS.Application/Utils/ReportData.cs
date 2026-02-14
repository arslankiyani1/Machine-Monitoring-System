namespace MMS.Application.Utils;

public class ReportData
{
    public Dictionary<string, string> MachineDetails { get; set; } = new();
    public Dictionary<string, double> Summary { get; set; } = new();
    public List<Dictionary<string, object>> DailyRows { get; set; } = new();
}