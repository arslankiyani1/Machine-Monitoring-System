namespace MMS.Adapters.Keycloak.Repositories;

public class AuthService(
    HttpClient httpClient,
    IOptions<KeycloakSettings> options) : IAuthService
{
    private readonly KeycloakSettings keyCloackConfig = options.Value;

    public async Task<ApiResponse<TokenResponseDto>> LoginAsync(LoginRequestDto request)
    {
        try
        {
            var content = new FormUrlEncodedContent(new[]
            {
            new KeyValuePair<string, string>("client_id",keyCloackConfig.ClientId),
            new KeyValuePair<string, string>("grant_type",Constant.password_grant_type),
            new KeyValuePair<string, string>("username", request.Username),  // Changed from Username
            new KeyValuePair<string, string>("password", request.Password),
            new KeyValuePair<string, string>("scope", Constant.scope)
            });

            var response = await httpClient.PostAsync(keyCloackConfig.TokenEndpoint, content);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                string errorMessage;
                try
                {
                    var errorJson = JsonSerializer.Deserialize<JsonElement>(errorBody);
                    errorMessage = errorJson.TryGetProperty("error_description", out var msg)
                        ? msg.GetString()! : errorBody;
                }
                catch
                {
                    errorMessage = errorBody;
                }

                return new ApiResponse<TokenResponseDto>((int)response.StatusCode, $"{errorMessage}");
            }

            var json = await response.Content.ReadAsStringAsync();

            var data = JsonSerializer.Deserialize<JsonElement>(json);

            var accessToken = data.GetProperty("access_token").GetString()!;
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(accessToken);
            var emailVerified = token.Claims.FirstOrDefault(c => c.Type == "email_verified")?.Value;

            if (emailVerified != "true")
            {
                return new ApiResponse<TokenResponseDto>(
                    StatusCodes.Status403Forbidden,
                    "Email not verified. Please verify your email before logging in."
                );
            }

            return new ApiResponse<TokenResponseDto>(
                 StatusCodes.Status200OK,
                 "Login Success",
                 new TokenResponseDto
                 (
                     data.GetProperty("access_token").GetString()!,
                     data.GetProperty("refresh_token").GetString()!,
                     data.GetProperty("expires_in").GetInt32(),
                     data.GetProperty("refresh_expires_in").GetInt32(),
                     data.GetProperty("token_type").GetString()!
                 )
             );
        }
        catch (Exception ex)
        {
            return new ApiResponse<TokenResponseDto>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred: {ex.Message}"
            );
        }
    }
    public async Task<ApiResponse<TokenResponseDto>> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            var content = new FormUrlEncodedContent(new[]
            {
                   new KeyValuePair<string, string>("client_id",keyCloackConfig.ClientId),
                   new KeyValuePair<string, string>("grant_type",Constant.refresh_token_grant_type),
                   new KeyValuePair<string, string>("refresh_token", refreshToken)
            });

            var response = await httpClient.PostAsync(keyCloackConfig.TokenEndpoint, content);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                string errorMessage;
                try
                {
                    var errorJson = JsonSerializer.Deserialize<JsonElement>(errorBody);
                    errorMessage = errorJson.TryGetProperty("error_description", out var msg)
                        ? msg.GetString()! : errorBody;
                }
                catch
                {
                    errorMessage = errorBody;
                }

                return new ApiResponse<TokenResponseDto>((int)response.StatusCode, $"{errorMessage}");
            }

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<JsonElement>(json);

            var accessToken = data.GetProperty("access_token").GetString()!;
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(accessToken);
            var emailVerified = token.Claims.FirstOrDefault(c => c.Type == "email_verified")?.Value;

            if (emailVerified != "true")
            {
                return new ApiResponse<TokenResponseDto>(
                    StatusCodes.Status403Forbidden,
                    "Email not verified. Please verify your email before logging in."
                );
            }

            return new ApiResponse<TokenResponseDto>(
                   StatusCodes.Status200OK,
                   "Refresh Token Success",
                   new TokenResponseDto
                   (
                       data.GetProperty("access_token").GetString()!,
                       data.GetProperty("refresh_token").GetString()!,
                       data.GetProperty("expires_in").GetInt32(),
                       data.GetProperty("refresh_expires_in").GetInt32(),
                       data.GetProperty("token_type").GetString()!
                   )
               );
        }
        catch (Exception ex)
        {
            return new ApiResponse<TokenResponseDto>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred: {ex.Message}"
            );
        }

    }

    public async Task<ApiResponse<string>> LogoutAsync(string refreshToken)
    {
        try
        {
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "client_id", keyCloackConfig.ClientId },
            { "refresh_token", refreshToken }
        });

            var response = await httpClient.PostAsync(keyCloackConfig.LogoutEndpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                string errorMessage;

                try
                {
                    var errorJson = JsonSerializer.Deserialize<JsonElement>(errorBody);
                    errorMessage = errorJson.TryGetProperty("error_description", out var msg)
                        ? msg.GetString()! : errorBody;
                }
                catch
                {
                    errorMessage = errorBody;
                }

                return new ApiResponse<string>((int)response.StatusCode, $"Logout failed: {errorMessage}");
            }

            return new ApiResponse<string>(
                StatusCodes.Status200OK,
                "Logout successful"
            );
        }
        catch (Exception ex)
        {
            return new ApiResponse<string>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred: {ex.Message}"
            );
        }
    }

}