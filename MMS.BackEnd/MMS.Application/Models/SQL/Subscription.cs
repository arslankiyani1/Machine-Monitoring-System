namespace MMS.Application.Models.SQL;

public class Subscription : Trackable
{
    public string Name { get; set; } = default!;
    public string Currency { get; set; } = "USD";
    public decimal Price { get; set; }
    public BillingCycle BillingCycle { get; set; }
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;
    public Dictionary<string, object> Features { get; set; } = default!;
}