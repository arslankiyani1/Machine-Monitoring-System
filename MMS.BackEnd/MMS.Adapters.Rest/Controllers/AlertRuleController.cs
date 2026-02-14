namespace MMS.Adapters.Rest.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlertRuleController(IAlertRuleService service) : BaseController
{ 
    #region Queries

    /// <summary>
    /// Gets all alert rules.
    /// </summary>
    /// <returns>List of all alert rules.</returns>
    [HttpGet("GetAll")]
    [SwaggerOperation(Summary = "Get all alert rules", Description = "Returns all alert rules from the database.")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<AlertRuleDto>>), StatusCodes.Status200OK)]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewerOrTechnicianOrOperatorOrMMSBridge)]
    public async Task<IActionResult> GetAll([FromQuery] PageParameters pageParameters,[FromQuery] Guid? machineId)
    {
        var response = await service.GetAllAsync(pageParameters,machineId);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Retrieves an alert rule by its ID.
    /// </summary>
    /// <param name="id">The ID of the alert rule.</param>
    /// <returns>The requested alert rule.</returns>
    [HttpGet("GetById/{id}")]
    [SwaggerOperation(Summary = "Get alert rule by ID", Description = "Fetches an alert rule using its ID.")]
    [ProducesResponseType(typeof(ApiResponse<AlertRuleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewerOrTechnicianOrOperatorOrMMSBridge)]
    public async Task<IActionResult> GetById(string id)
    {
        var response = await service.GetByIdAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    #endregion

    #region Commands

    /// <summary>
    /// Creates a new alert rule.
    /// </summary>
    /// <param name="dto">The alert rule data to create.</param>
    /// <returns>The newly created alert rule.</returns>
    [HttpPost("Create")]
    [SwaggerOperation(Summary = "Create alert rule", Description = "Adds a new alert rule.")]
    [ProducesResponseType(typeof(ApiResponse<AlertRule>), StatusCodes.Status201Created)]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewerOrTechnicianOrOperatorOrMMSBridge)]
    public async Task<IActionResult> Create([FromBody] AddAlertRuleDto dto)
    {
        var response = await service.CreateAsync(dto);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Updates an existing alert rule.
    /// </summary>
    /// <param name="dto">The alert rule update object.</param>
    [HttpPut("Update")]
    [SwaggerOperation(Summary = "Update alert rule", Description = "Updates an existing alert rule.")]
    [ProducesResponseType(typeof(ApiResponse<AlertRule>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewerOrTechnicianOrOperatorOrMMSBridge)]
    public async Task<IActionResult> Update([FromBody] UpdateAlertRuleDto dto)
    {
        var response = await service.UpdateAsync(dto);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Deletes an alert rule by ID.
    /// </summary>
    /// <param name="id">The ID of the alert rule to delete.</param>
    [HttpDelete("Delete/{id}")]
    [SwaggerOperation(Summary = "Delete alert rule", Description = "Deletes an alert rule entry by its ID.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewerOrTechnicianOrOperatorOrMMSBridge)]
    public async Task<IActionResult> Delete(string id)
    {
        var response = await service.DeleteAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    #endregion
}