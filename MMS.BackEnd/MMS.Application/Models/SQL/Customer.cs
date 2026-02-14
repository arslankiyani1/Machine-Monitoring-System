namespace MMS.Application.Models.SQL;

public class Customer : Trackable
{
    public string Name { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string PhoneCountryCode { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
    public string TimeZone { get; set; } = default!;
    public string Country { get; set; } = default!;
    public string City { get; set; } = default!;
    public string Street { get; set; } = default!;
    public string PostalCode { get; set; } = default!;
    public string Region { get; set; } = default!;
    public string? State { get; set; } = default!;
    public string? ImageUrls { get; set; } = default!;
    public CustomerStatus Status { get; set; }
    public List<Shift> Shifts { get; set; } = default!;

    #region Navigatonal Properties
    
    public CustomerDashboard CustomerDashboard { get; set; } = default!;
    public ICollection<Machine> Machine { get; set; } = [];
    public ICollection<CustomerReportSetting> CustomerReportSettings { get; set; } = [];
 
    #endregion
}

public class Shift
{
    public string ShiftName { get; set; } = default!;
    public DateTime Start { get; set; } = default!;
    public DateTime End { get; set; } = default!;
}