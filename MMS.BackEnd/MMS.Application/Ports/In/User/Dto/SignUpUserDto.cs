namespace MMS.Application.Ports.In.User.Dto;

public record SignUpUserDto(
    string Username,
    string Email,
    string Password,
    List<Guid> CustomerIds,
    string? ProfileImageBase64,
    string? PhoneCode,
    string? PhoneNumber,
    Language? Language
);