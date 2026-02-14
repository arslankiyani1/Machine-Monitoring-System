namespace MMS.Adapters.Rest.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : BaseController
{
    /// <summary>
    /// Authenticates the user and returns access and refresh tokens.
    /// </summary>
    /// <param name="request">The login request containing username/email and password.</param>
    /// <returns>Returns a token response if authentication succeeds.</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<TokenResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [SwaggerOperation(
        Summary = "User login",
        Description = "Authenticates the user using username/email and password, and returns JWT access and refresh tokens.")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        if (request == null)
            return BadRequest(new ApiResponse<string>(StatusCodes.Status400BadRequest, "Request body cannot be null"));

        var response = await authService.LoginAsync(request);
        return StatusCode(response.StatusCode, response);
    }


    /// <summary>
    /// Refreshes the access token using a valid refresh token.
    /// </summary>
    /// <param name="Dto">The refresh token DTO.</param>
    /// <returns>Returns a new access token.</returns>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(ApiResponse<TokenResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [SwaggerOperation(
        Summary = "Refresh access token",
        Description = "Generates a new access token using a valid refresh token.")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto Dto)
    {
        if (Dto.RefreshToken == null)
            return BadRequest(new ApiResponse<string>(StatusCodes.Status400BadRequest, "Request body cannot be null"));

        var response = await authService.RefreshTokenAsync(Dto.RefreshToken);
        return StatusCode(response.StatusCode, response);
    }


    /// <summary>
    /// Logs the user out by invalidating the refresh token.
    /// </summary>
    /// <param name="Dto">The refresh token DTO.</param>
    /// <returns>Returns a success response on successful logout.</returns>
    [HttpPost("logout")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [SwaggerOperation(
        Summary = "Logout user",
        Description = "Logs out the user by revoking the provided refresh token.")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenDto Dto)
    {
        if (Dto.RefreshToken == null)
            return BadRequest(new ApiResponse<string>(StatusCodes.Status400BadRequest, "Request body cannot be null"));

        var response = await authService.LogoutAsync(Dto.RefreshToken);
        return StatusCode(response.StatusCode, response);
    }
}
