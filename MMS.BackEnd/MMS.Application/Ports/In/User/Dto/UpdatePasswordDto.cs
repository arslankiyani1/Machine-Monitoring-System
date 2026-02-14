namespace MMS.Application.Ports.In.User.Dto;


public record UpdatePasswordDto
{
  
    public Guid Id { get; init; }
    public string OldPassword { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;
}
