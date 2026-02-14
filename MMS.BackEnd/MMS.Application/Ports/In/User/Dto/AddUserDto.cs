namespace MMS.Application.Ports.In.User.Dto;

public record AddUserDto(
    string Username,
    string Email,
    string? FirstName,
    string? LastName,
    string Password,
    List<Guid>? CustomerIds,
    List<Guid>? MachineIds,
    string? ProfileImageBase64,
    List<string>? FcmTokens,
    string? PhoneCode,
    string? PhoneNumber,
    string? City,
    string? Country,
    string? Region,
    string? State,
    string? TimeZone,
    string? JobTitle,
    string? Department,
    string? Role,
    Language? Language)
{
    public AddUserDto() : this(
        "", "", null, null, "", null, null, null, null, null,
        null, null, null, null, null, null, null, null, null, null)
    { }
}