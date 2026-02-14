namespace MMS.Application.Models.Keycloak;

public class UserModel
{
    public Guid UserId { get; set; } = default!;
    public string Username { get; set; } = default!;
    public string? Email { get; set; }
    public bool Enabled { get; set; }
    public bool EmailVerified { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public List<string>? CustomerIds { get; set; }
    public string? ProfileImage { get; set; }
    public List<string>? FcmTokens { get; set; }
    public string? PhoneCode { get; set; }
    public string? PhoneNumber { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? Region { get; set; }
    public string? State { get; set; }
    public string? TimeZone { get; set; }
    public string? JobTitle { get; set; }
    public string? Department { get; set; }
    public string? Role { get; set; }
    public string? RoleDisplayName { get; set; }
    public Language Language { get; set; }
    public IEnumerable<MachineDto>? Machines { get; set; }
}