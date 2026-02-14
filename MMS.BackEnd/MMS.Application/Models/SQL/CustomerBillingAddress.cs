namespace MMS.Application.Models.SQL;

public class CustomerBillingAddress : Trackable
{
    public string Country { get; set; } = default!;
    public string Region { get; set; } = default!;
    public string ZipCode { get; set; } = default!;
    public string City { get; set; } = default!;
    public string State { get; set; } = default!;
    public string street { get; set; } = default!;
    public Guid CustomerId { get; set; }
}
