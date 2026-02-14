namespace MMS.Application.Ports.In.Customer.Dto;

public class CustomerCardDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? ImageUrl { get; set; }
    public int TotalMachines { get; set; }
    public string TimeZone { get; set; } = default!; // Customer's Time Zone
    public List<ShiftDto> Shifts { get; set; } = default!; // Add this
    public Dictionary<string, int> StatusSummary { get; set; } = new(); 

}


public class ShiftDto
{
    public string ShiftName { get; set; } = default!;
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
}