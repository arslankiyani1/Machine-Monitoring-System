namespace MMS.Adapters.Rest.Controllers;

[Route("api/[controller]")]
[ApiController]
// [Authorize(Roles = ApplicationRoles.User)]
public class MachineSettingController(IMachineSettingService machineSettingService) : BaseController
{
    #region Queries

    /// <summary>
    /// Retrieves a list of all machine settings.
    /// </summary>
    /// <returns>A list of all machine settings.</returns>
    [HttpGet("getAllMachineSettings")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<MachineSettingDto>>), StatusCodes.Status200OK)]
    [SwaggerOperation(Summary = "Retrieves a list of all machine settings.", Description = "Fetches all machine settings available in the system.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewerOrTechnicianOrOperator)]
    public async Task<IActionResult> GetListAsync([FromQuery] PageParameters pageParameters,
        [FromQuery] MachineSettingsStatus? status, [FromQuery] Guid? MachineId)
    {
        var response = await machineSettingService.GetListAsync(pageParameters,status,MachineId);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Retrieves a machine setting by ID.
    /// </summary>
    [HttpGet("getById/{id:Guid}")]
    [ProducesResponseType(typeof(ApiResponse<MachineSettingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Retrieves a machine setting by ID.", Description = "Fetches a specific machine setting in the system by its unique ID.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewerOrTechnicianOrOperator)]
    public async Task<IActionResult> GetByIdAsync(Guid id)
    {
        var response = await machineSettingService.GetByIdAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    #endregion

    #region Commands

    /// <summary>
    /// Adds a new machine setting.
    /// </summary>
    /// <param name="addMachineSettingRequest">The data for creating the machine setting.</param>
    /// <returns>The newly added machine setting.</returns>
    [HttpPost("createMachineSetting")]
    [ProducesResponseType(typeof(ApiResponse<MachineSettingDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [SwaggerOperation(Summary = "Adds a new machine setting.", Description = "Registers a new machine setting with the provided details.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrTechnician)]
    public async Task<IActionResult> AddAsync([FromBody] AddMachineSettingDto addMachineSettingRequest)
    {
        if (addMachineSettingRequest == null)
            return BadRequest(new ApiResponse<string>(StatusCodes.Status400BadRequest, "Request body cannot be null"));

        var response = await machineSettingService.AddAsync(addMachineSettingRequest);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Updates an existing machine setting by its ID.
    /// </summary>
    /// <param name="updateMachineSettingRequest">The data for updating the machine setting.</param>
    /// <returns>The updated machine setting.</returns>
    [HttpPut("updateMachineSetting")]
    [ProducesResponseType(typeof(ApiResponse<MachineSettingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Updates an existing machine setting.", Description = "Updates the details of an existing machine setting by its ID.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrTechnician)]
    public async Task<IActionResult> UpdateAsync([FromBody] UpdateMachineSettingDto updateMachineSettingRequest)
    {
        if (updateMachineSettingRequest == null)
            return BadRequest(new ApiResponse<string>(StatusCodes.Status400BadRequest, "Invalid request. Machine setting ID is required."));

        var response = await machineSettingService.UpdateAsync(updateMachineSettingRequest.Id, updateMachineSettingRequest);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Deletes a machine setting by its ID.
    /// </summary>
    /// <param name="id">The ID of the machine setting to be deleted.</param>
    /// <returns>A 204 No Content response if successful.</returns>
    [HttpDelete("deleteMachineSetting/{id:Guid}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Deletes a machine setting.", Description = "Removes a machine setting from the system by its ID.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdmin)]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var response = await machineSettingService.DeleteAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    #endregion
}