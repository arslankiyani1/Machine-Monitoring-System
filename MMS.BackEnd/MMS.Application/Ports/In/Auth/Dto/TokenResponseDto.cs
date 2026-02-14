namespace MMS.Application.Ports.In.Auth.Dto;

public record TokenResponseDto(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    int RefreshTokenExpiresIn,
    string TokenType
);
