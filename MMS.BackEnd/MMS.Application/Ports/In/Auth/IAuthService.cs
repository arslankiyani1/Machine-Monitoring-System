namespace MMS.Application.Ports.In.Auth;

public interface IAuthService
{
    Task<ApiResponse<TokenResponseDto>> LoginAsync(LoginRequestDto request);
    Task<ApiResponse<TokenResponseDto>> RefreshTokenAsync(string refreshToken);
    Task<ApiResponse<string>> LogoutAsync(string refreshToken);
}
