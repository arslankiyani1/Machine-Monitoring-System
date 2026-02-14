namespace MMS.Application.Models.SQL;

public class CustomerSubscription : Trackable
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive {  get; set; }
    public RenewalType RenewalType { get; set; }
    public string Status { get; set; } = default!;

    public Guid CustomerId { get; set; }
    public Guid SubscriptionId { get; set; }
    public Guid InvoiceId {  get; set; }
}