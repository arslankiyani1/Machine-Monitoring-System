namespace MMS.Adapters.Rest.Controllers;

[Route("api/[controller]")]
[ApiController]
// [Authorize(Roles = ApplicationRoles.User)]
public class WidgetsController(IWidgetService widgetService) : BaseController
{
    #region Queries

    /// <summary>
    /// Retrieves a list of all widgets.
    /// </summary>
    /// <returns>A list of all widgets.</returns>
    [HttpGet("getAllWidgets")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<WidgetDto>>), StatusCodes.Status200OK)]
    [SwaggerOperation(Summary = "Retrieves a list of all widgets.", Description = "Fetches all widgets available in the system.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewerOrTechnicianOrOperator)]
    public async Task<IActionResult> GetListAsync([FromQuery] PageParameters pageParameters, 
        [FromQuery] WidgetType? WidgetType, [FromQuery] WidgetSourceType? SourceType)
    {
        var response = await widgetService.GetListAsync(pageParameters, WidgetType, SourceType);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Retrieves a widget by ID.
    /// </summary>
    [HttpGet("getById/{id:Guid}")]
    [ProducesResponseType(typeof(ApiResponse<WidgetDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Retrieves a widget by ID.", Description = "Fetches a specific widget in the system by its unique ID.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewerOrTechnicianOrOperator)]
    public async Task<IActionResult> GetByIdAsync(Guid id)
    {
        var response = await widgetService.GetByIdAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    #endregion

    #region Commands

    /// <summary>
    /// Adds a new widget.
    /// </summary>
    /// <param name="addWidgetRequest">The data for creating the widget.</param>
    /// <returns>The newly added widget.</returns>
    [HttpPost("createWidget")]
    [ProducesResponseType(typeof(ApiResponse<WidgetDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [SwaggerOperation(Summary = "Adds a new widget.", Description = "Registers a new widget with the provided details.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdmin)]
    public async Task<IActionResult> AddAsync([FromBody] AddWidgetDto addWidgetRequest)
    {
        if (addWidgetRequest == null)
            return BadRequest(new ApiResponse<string>(StatusCodes.Status400BadRequest, "Request body cannot be null"));

        var response = await widgetService.AddAsync(addWidgetRequest);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Updates an existing widget by its ID.
    /// </summary>
    /// <param name="id">The ID of the widget to be updated.</param>
    /// <param name="updateWidgetRequest">The data for updating the widget.</param>
    /// <returns>The updated widget.</returns>
    [HttpPut("updateWidget")]
    [ProducesResponseType(typeof(ApiResponse<WidgetDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Updates an existing widget.", Description = "Updates the details of an existing widget by its ID.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdmin)]
    public async Task<IActionResult> UpdateAsync([FromBody] UpdateWidgetDto updateWidgetRequest)
    {
        if (updateWidgetRequest == null)
            return BadRequest(new ApiResponse<string>(StatusCodes.Status400BadRequest, "Invalid request. Widget ID is required."));

        var response = await widgetService.UpdateAsync(updateWidgetRequest.Id, updateWidgetRequest);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Deletes a widget by its ID.
    /// </summary>
    /// <param name="id">The ID of the widget to be deleted.</param>
    /// <returns>A 204 No Content response if successful.</returns>
    [HttpDelete("deleteWidget/{id:Guid}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Deletes a widget.", Description = "Removes a widget from the system by its ID.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdmin)]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var response = await widgetService.DeleteAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    #endregion
}