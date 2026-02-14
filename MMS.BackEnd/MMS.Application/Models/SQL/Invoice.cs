namespace MMS.Application.Models.SQL;

public class Invoice : Trackable
{
    public string Invoicenumber { get; set; } = default!;
    public DateTime Payment {  get; set; }
    public decimal Amout { get; set; }
    public decimal Tax { get; set; }
    public string Status { get; set; } = default!;
    public string Paymentmethod { get; set; } = default!; 
    public string PaymentGatewayTrxId { get; set; } = default!;

    public Guid CustomerSubscriptionId { get; set; } = default!;
    public Guid CustomerId { get; set; } = default!;
    public Guid BillingAdressId { get; set; } = default!;
}
