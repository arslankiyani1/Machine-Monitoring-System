using MMS.Application.Models.Keycloak;
using MMS.Application.Services;

namespace MMS.Adapters.Rest.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController(IUserService userService) : BaseController
{
    #region Queries

    /// <summary>
    /// Retrieves the currently authenticated user's information.
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiResponse<UserModel>), StatusCodes.Status200OK)]
    [SwaggerOperation(Summary = "Get current user info", Description = "Returns information about the currently authenticated user.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewerOrTechnicianOrOperator)]
    public async Task<IActionResult> GetMe()
    {
        var result = await userService.GetCurrentUserAsync();
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Retrieves all users with roles, filtering, and pagination.
    /// </summary>
    [HttpGet("all")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserModel>>), StatusCodes.Status200OK)]
    [SwaggerOperation(Summary = "Get all users", Description = "Returns paginated and filtered list of users with roles.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewerOrTechnicianOrOperator)]
    public async Task<IActionResult> GetAllUsers([FromQuery] UserQueryParameters query)
    {
        var result = await userService.GetAllUsersAsync(query, false); // false so dont show superadmin
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Retrieves a summary of all users associated with a specific customer,
    /// including assigned and unassigned users.
    /// </summary>
    /// <param name="customerId">The ID of the customer.</param>
    /// <returns>
    /// A summary object showing total users, how many are assigned to machines,
    /// how many are unassigned, and how many are unavailable (currently always 0).
    /// </returns>
    [HttpGet("CustomerUsersSummary/{customerId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CustomerUsersSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CustomerUsersSummaryDto>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<CustomerUsersSummaryDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<CustomerUsersSummaryDto>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Get user summary for a customer",
        Description = "Returns a summary of all users for the specified customer, including total, assigned, unassigned, and unavailable users."
    )]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdmin)]
    public async Task<IActionResult> GetCustomerUsersSummary( Guid customerId)
    {
        var result = await userService.GetCustomerUsersSummary(customerId);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Retrieves user details and roles by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<UserModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [SwaggerOperation(Summary = "Get user by ID", Description = "Returns user detail and roles using Keycloak ID.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdmin)]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        if (id == Guid.Empty)
            return BadRequest(new ApiResponse<string>(400, "Invalid user ID"));

        var result = await userService.GetUserByIdAsync(id);
        return StatusCode(result.StatusCode, result);
    }


    /// <summary>
    /// Retrieves all available realm roles in Keycloak.
    /// </summary>
    /// <returns>A list of all realm roles with id, name, and description.</returns>
    [HttpGet("roles")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<RoleDto>>), StatusCodes.Status200OK)]
    [SwaggerOperation(
        Summary = "Get all realm roles",
        Description = "Returns a list of all roles defined in the Keycloak realm, including their ID, name, and description."
    )]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdmin)]
    public async Task<IActionResult> GetAllRoles()
    {
        var result = await userService.GetAllRealmRolesAsync();
        return StatusCode(result.StatusCode, result);
    }
    #endregion

    #region Commands
    /// <summary>
    /// Signs up a new user (account disabled, email verification required).
    /// </summary>
    [HttpPost("signup")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [SwaggerOperation(Summary = "User signup", Description = "Registers a user with email verification and disabled account.")]
    public async Task<IActionResult> Signup([FromBody] SignUpUserDto request)
    {
        if (request == null)
            return BadRequest(new ApiResponse<string>(StatusCodes.Status400BadRequest, "Request body cannot be null"));
        var dto = userService.ConvertToAddUserDto(request);
        var response = await userService.CreateUserAsync(dto, isSignUp: true);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Updates user's password.
    /// </summary>
    [HttpPut("update-password")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
    [SwaggerOperation(Summary = "Update user password", Description = "Updates the user's password after verifying the old password.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewerOrTechnicianOrOperatorOrMMSBridge)]
    public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordDto request)
    {
        if (request == null)
            return BadRequest(new ApiResponse<string>(StatusCodes.Status400BadRequest, "Request body cannot be null"));
        var response = await userService.UpdatePasswordAsync(request.Id, request.OldPassword, request.NewPassword);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Creates and approves a user directly (enabled & verified).
    /// </summary>
    [HttpPost("create")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [SwaggerOperation(Summary = "Create approved user", Description = "Admin creates user who is enabled and email-verified.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdmin)]
    public async Task<IActionResult> Create([FromBody] AddUserDto request)
    {
        if (request == null)
            return BadRequest(new ApiResponse<string>(StatusCodes.Status400BadRequest, "Request body cannot be null"));

        var response = await userService.CreateUserAsync(request, isSignUp: false);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Assigns one or more realm roles to the specified user.
    /// </summary>
    /// <param name="id">The Keycloak user ID.</param>
    /// <param name="dto">A list of role names to assign.</param>
    /// <returns>200 if successful, 400 if any role or ID is invalid.</returns>
    [HttpPost("assign-role")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [SwaggerOperation(
        Summary = "Assign roles to user",
        Description = "Assigns realm role to a specific user in Keycloak."
    )]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdmin)]
    public async Task<IActionResult> AssignRoles([FromBody] AssignRolesDto dto)
    {
        if (dto == null)
        {
            return BadRequest(new ApiResponse<string>(StatusCodes.Status400BadRequest, "Request body cannot be null"));
        }

        var result = await userService.AssignRoleToUserAsync(dto);
        return StatusCode(result.StatusCode, result);
    }


    /// <summary>
    /// Enables a disabled user account.
    /// </summary>
    [HttpPut("enable/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [SwaggerOperation(Summary = "Enable user", Description = "Enables a previously disabled user account by ID.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdmin)]
    public async Task<IActionResult> EnableAccount(Guid id)
    {
        if (id == Guid.Empty)
            return BadRequest(new ApiResponse<string>(400, "User ID is required"));

        var result = await userService.EnableUserAsync(id);
        return StatusCode(result.StatusCode, result);
    }



    /// <summary>
    /// Verify User account email.
    /// </summary>
    [HttpPut("verify-email/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [SwaggerOperation(Summary = "Verify user email", Description = "Verifies the email of a user account by ID.")]
    //[Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdmin)]
    public async Task<IActionResult> VerifyEmail(Guid id)
    {
        if (id == Guid.Empty)
            return BadRequest(new ApiResponse<string>(400, "User ID is required"));

        var result = await userService.VerifyEmailAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Reset password for a Keycloak user.
    /// </summary>
    [HttpPut("{userId:guid}/reset-password")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [SwaggerOperation(Summary = "Reset User Password", Description = "Endpoint to enable admin to reset user password.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdmin)]
    public async Task<IActionResult> ResetPassword(Guid userId, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8)
        {
            return BadRequest(new ApiResponse<string>(
                StatusCodes.Status400BadRequest,
                "NewPassword is required and must be at least 8 characters."));
        }

        var result = await userService.ResetPasswordAsync(userId, newPassword);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Disables an enabled user account.
    /// </summary>
    [HttpPut("disable/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [SwaggerOperation(Summary = "Disable user", Description = "Disables a user account by ID.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdmin)]
    public async Task<IActionResult> DisableAccount(Guid id)
    {
        if (id == Guid.Empty)
            return BadRequest(new ApiResponse<string>(400, "User ID is required"));

        var result = await userService.DisableUserAsync(id); 
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Updates the editable fields of an existing user.
    /// </summary>
    /// <param name="dto">Fields to update. Only non-empty values are applied.</param>
    /// <returns>200 if successful, 400 if invalid input or failure.</returns>
    [HttpPut("update")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [SwaggerOperation(
        Summary = "Update user",
        Description = "Updates non-null fields for a user in Keycloak. Existing values are preserved unless overridden."
    )]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewerOrTechnicianOrOperatorOrMMSBridge)]
    public async Task<IActionResult> UpdateUser([FromBody] UpdateUserDto dto)
    {
        if (dto.UserId == Guid.Empty)
            return BadRequest(new ApiResponse<string>(400, "Invalid user ID"));

        var result = await userService.UpdateUserAsync(dto.UserId, dto);
        return StatusCode(result.StatusCode, result);
    }



    /// <summary>
    /// Deletes a user account by ID.
    /// </summary>
    [HttpDelete("delete/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [SwaggerOperation(Summary = "Delete user", Description = "Deletes a user account from Keycloak by ID.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdmin)]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        if (id == Guid.Empty)
            return BadRequest(new ApiResponse<string>(400, "User ID is required"));

        var result = await userService.DeleteUserAsync(id);
        return StatusCode(result.StatusCode, result);
    }
    #endregion
}