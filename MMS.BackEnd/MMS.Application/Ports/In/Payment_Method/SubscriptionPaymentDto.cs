namespace MMS.Application.Ports.In.Payment_Method;

public class SubscriptionPaymentDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public RenewalType RenewalType { get; set; }
    public string SubscriptionStatus { get; set; } = default!;
    public Guid SubscriptionId { get; set; }

    public string InvoiceNumber { get; set; } = default!;
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public decimal Tax { get; set; }
    public string InvoiceStatus { get; set; } = default!;
    public string PaymentMethod { get; set; } = default!;
    public string PaymentGatewayTrxId { get; set; } = default!;

    public Guid CustomerId { get; set; }
    public Guid BillingAddressId { get; set; }
}
