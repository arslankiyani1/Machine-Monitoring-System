namespace MMS.Adapters.Rest.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MachineStatusSettingsController(IMachineStatusSettingService service) : BaseController
{
    #region Queries

    /// <summary>
    /// Retrieves all machine status settings.
    /// </summary>
    [HttpGet("getAll")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<MachineStatusSetting>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Get all machine status settings", Description = "Returns a list of all machine status settings.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewerOrTechnicianOrOperator)]
    public async Task<IActionResult> GetAllAsync()
    {
        var response = await service.GetAllAsync();
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Retrieves a machine status setting by ID.
    /// </summary>
    [HttpGet("getById/{id}")]
    [ProducesResponseType(typeof(ApiResponse<MachineStatusSetting>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Get machine status setting by ID", Description = "Returns a machine status setting for a given ID.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewerOrTechnicianOrOperator)]
    public async Task<IActionResult> GetByIdAsync(string id)
    {
        var response = await service.GetByIdAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Retrieves a machine status setting by machine ID.
    /// </summary>
    [HttpGet("getByMachineId")]
    [ProducesResponseType(typeof(ApiResponse<MachineStatusSetting>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Get machine status setting by machine ID", Description = "Returns a machine status setting for a given machine ID.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewerOrTechnicianOrOperator)]
    public async Task<IActionResult> GetByMachineIdAsync([FromQuery] Guid machineId)
    {
        var response = await service.GetByMachineIdAsync(machineId);
        return StatusCode(response.StatusCode, response);
    }

    #endregion

    #region Commands

    /// <summary>
    /// Creates a new machine status setting.
    /// </summary>
    [HttpPost("create")]
    [ProducesResponseType(typeof(ApiResponse<MachineStatusSetting>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [SwaggerOperation(Summary = "Create machine status setting", Description = "Creates a new machine status setting.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrTechnician)]
    public async Task<IActionResult> CreateAsync([FromBody] MachineStatusSetting model)
    {
        if (model == null)
            return BadRequest(new ApiResponse<string>(StatusCodes.Status400BadRequest, "Request body cannot be null"));

        var response = await service.CreateAsync(model);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Updates an existing machine status setting.
    /// </summary>
    [HttpPut("update")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [SwaggerOperation(Summary = "Update machine status setting", Description = "Updates an existing machine status setting by ID.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrTechnician)]
    public async Task<IActionResult> UpdateAsync([FromBody] MachineStatusSetting model)
    {
        if (model == null)
            return BadRequest(new ApiResponse<string>(StatusCodes.Status400BadRequest, "Request body cannot be null"));

        var response = await service.UpdateAsync( model.Id,model);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Deletes a machine status setting by ID.
    /// </summary>
    [HttpDelete("delete/{id}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Delete machine status setting", Description = "Deletes a machine status setting by ID.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdmin)]
    public async Task<IActionResult> DeleteAsync(string id)
    {
        var response = await service.DeleteAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Retrieves all distinct machine statuses from Cosmos DB.
    /// </summary>
    [HttpGet("getAllStatuses")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<string>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Get all machine statuses",
        Description = "Returns a distinct list of all machine statuses found in Cosmos DB across all machines."
    )]
    [AllowAnonymous]
    //[Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewerOrTechnicianOrOperator)]
    public async Task<IActionResult> GetAllStatusesAsync()
    {
        var response = await service.GetAllStatusesAsync();
        return StatusCode(response.StatusCode, response);
    }

    #endregion
}
