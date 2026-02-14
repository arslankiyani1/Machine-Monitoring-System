namespace MMS.Application.Models.NoSQL;

public class CustomerDashboardSummary
{
    public string Id { get; set; } = default!;
    public string CustomerId { get; set; } = default!;
    public  Dictionary<string, int> StatusSummary { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Dictionary<string, int> DowntimeSummary { get; set; } = new();
}
