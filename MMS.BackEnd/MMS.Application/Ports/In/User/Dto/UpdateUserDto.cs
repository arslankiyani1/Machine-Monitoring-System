namespace MMS.Application.Ports.In.User.Dto;
public record UpdateUserDto(
    Guid UserId,
    string? FirstName,
    string? LastName,
    bool? Enabled,
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
    Language? Language
);
