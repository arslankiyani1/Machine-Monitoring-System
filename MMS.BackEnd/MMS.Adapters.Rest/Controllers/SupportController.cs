namespace MMS.Adapters.Rest.Controllers;

[Route("api/[controller]")]
[ApiController]
// [Authorize(Roles = ApplicationRoles.User)]
public class SupportController(ISupportService supportService) : BaseController
{

    /// <summary>
    /// Retrieves a list of all support tickets.
    /// </summary>
    [HttpGet("list")]
    [ProducesResponseType(typeof(ApiResponse<List<SupportTicketDto>>), StatusCodes.Status200OK)]
    [SwaggerOperation(
        Summary = "Get all support tickets",
        Description = "Returns a list of all support tickets."
    )]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewerOrTechnicianOrOperator)]
    public async Task<IActionResult> GetAllAsync([FromQuery] Guid customerId, [FromQuery] PageParameters pageParameters)
    {
        var response = await supportService.GetListAsync(customerId,pageParameters);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Retrieves a support ticket by its ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<SupportTicketDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(
        Summary = "Get support ticket by ID",
        Description = "Returns the support ticket details for the given ID."
    )]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrTechnicianOrOperator)]

    public async Task<IActionResult> GetByIdAsync(Guid id)
    {
        var response = await supportService.GetByIdAsync(id);
        return StatusCode(response.StatusCode, response);
    }


    #region Commands

    /// <summary>
    /// Creates a new support ticket.
    /// </summary>
    [HttpPost("create")]
    [ProducesResponseType(typeof(ApiResponse<SupportTicketDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [SwaggerOperation(
            Summary = "Create a new support ticket",
            Description = "Submits a new support request with optional attachment."
        )]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewerOrTechnicianOrOperator)]
    public async Task<IActionResult> AddAsync([FromForm] AddSupportTicketDto request)
    {
        if (request is null)
            return BadRequest(new ApiResponse<string>(StatusCodes.Status400BadRequest, "Request body cannot be null"));

        var response = await supportService.AddAsync(request);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Updates an existing support ticket.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<SupportTicketDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(
        Summary = "Update support ticket by ID",
        Description = "Updates the support ticket information."
    )]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrTechnicianOrOperator)]
    public async Task<IActionResult> UpdateAsync(Guid id, [FromForm] UpdateSupportTicketDto request)
    {
        var response = await supportService.UpdateAsync(id, request);
        return StatusCode(response.StatusCode, response);
    }


    /// <summary>
    /// Deletes a support ticket by its ID.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(
        Summary = "Delete support ticket by ID",
        Description = "Deletes the support ticket if it exists."
    )]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrTechnicianOrOperator)]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var response = await supportService.DeleteAsync(id);
        return StatusCode(response.StatusCode, response);
    }


    /// <summary>
    /// Marks a support ticket as resolved.
    /// </summary>
    [HttpPut("{id:guid}/resolve")]
    [ProducesResponseType(typeof(ApiResponse<SupportTicketDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(
        Summary = "Resolve a support ticket",
        Description = "Marks the support ticket as resolved."
    )]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewerOrTechnicianOrOperator)]
    public async Task<IActionResult> MarkAsResolvedAsync(Guid id)
    {
        var response = await supportService.MarkAsResolvedAsync(id);
        return StatusCode(response.StatusCode, response);
    }


    #endregion
}
