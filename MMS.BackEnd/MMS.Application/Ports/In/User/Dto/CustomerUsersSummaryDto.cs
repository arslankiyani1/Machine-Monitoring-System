namespace MMS.Application.Ports.In.User.Dto;

public class CustomerUsersSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? ImageUrl { get; set; }
    public int TotalUsers { get; set; }
    public int Assigned { get; set; }
    public int UnAssigned { get; set; }
    public int Unavailable { get; set; }
    public int Enabled { get; set; }   
    public int Disabled { get; set; }

}