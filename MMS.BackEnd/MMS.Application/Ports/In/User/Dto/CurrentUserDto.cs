namespace MMS.Application.Ports.In.User.Dto;
public class CurrentUserDto
{
    public Guid? UserId { get; set; }
    public string? Email { get; set; }
    public string? JobTitle { get; set; }
    public string? Department { get; set; }
    public string? CustomerId { get; set; }
    public List<string>? Roles { get; set; }
    public string? ImageUrl { get; set; }
}