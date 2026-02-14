namespace MMS.Application.Ports.In.Auth.Dto;

public record LoginRequestDto(
    string Username,   // Changed from Username
    string Password
);