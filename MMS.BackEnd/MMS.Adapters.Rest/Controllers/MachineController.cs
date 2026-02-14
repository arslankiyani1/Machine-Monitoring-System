namespace MMS.Adapters.Rest.Controllers;

[Route("api/[controller]")]
[ApiController]
// [Authorize(Roles = ApplicationRoles.User)]
public class MachineController(IMachineService machineService) : BaseController
{
    #region Queries
    /// <summary>
    /// Retrieves a list of all machines.
    /// </summary>
    /// <returns>A list of all machines.</returns>
    [HttpGet("getAllMachines")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<MachineDto>>), StatusCodes.Status200OK)]
    [SwaggerOperation(Summary = "Retrieves a list of all machines.", Description = "Fetches all machines available in the system.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewerOrTechnicianOrOperator)]
    public async Task<IActionResult> GetListAsync([FromQuery] PageParameters pageParameters, 
        [FromQuery] CommunicationProtocol? protocol, [FromQuery] MachineType? type, [FromQuery] Guid CustomerId)
    {
        var response = await machineService.GetListAsync(pageParameters,protocol,type,CustomerId);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Retrieves a machine by ID.
    /// </summary>
    [HttpGet("getById/{id:Guid}")]
    [ProducesResponseType(typeof(ApiResponse<MachineDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Retrieves a machine by ID.",
        Description = "Fetches a specific machine in the system by its unique ID.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewerOrTechnicianOrOperator)]
    public async Task<IActionResult> GetByIdAsync(Guid id)
    {
        var response = await machineService.GetByIdAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    [HttpGet("getSummaryById/{id:Guid}")]
    [ProducesResponseType(typeof(ApiResponse<MachineJobSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Retrieves machine summary by ID.",
    Description = "Fetches summarized or specific information for a machine by its unique ID.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewerOrTechnicianOrOperator)]
    public async Task<IActionResult> GetSummaryByIdAsync(Guid id,[FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] TimeRange range)
    {
        var response = await machineService.GetSummaryByIdAsync(id, range, from, to);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Retrieves detailed machine data by ID, including jobs, logs, and OEE metrics.
    /// </summary>
    [HttpGet("getDetailsById/{id:Guid}")]
    [ProducesResponseType(typeof(ApiResponse<MachineJobDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Retrieves detailed machine data by ID.",
        Description = "Fetches detailed information for a specific machine, including jobs, logs, and OEE metrics.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewerOrTechnicianOrOperator)]
    public async Task<IActionResult> GetMachineDetailsByIdAsync(Guid id)
    {
        var response = await machineService.GetMachineDetailsByIdAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    #endregion

    #region Commands

    /// <summary>
    /// Creates default machine settings for a specific machine.
    /// </summary>
    /// <param name="machineId">The unique identifier of the machine.</param>
    /// <returns>The default machine settings created for the machine.</returns>
    [HttpPost("{machineId}")]
    [ProducesResponseType(typeof(ApiResponse<MachineSettingDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Creates default machine settings.",
        Description = "Initializes a machine with its default settings if they don’t already exist.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdmin)]
    public async Task<IActionResult> CreateDefaultSettings(Guid machineId)
    {
        if (machineId == Guid.Empty)
            return BadRequest(new ApiResponse<string>(
                StatusCodes.Status400BadRequest,
                "Invalid machine ID."));

        var response = await machineService.CreateDefaultSettingsForMachineAsync(machineId);
        return StatusCode(response.StatusCode, response);
    }


    /// <summary>
    /// Adds a new machine.
    /// </summary>
    /// <param name="addMachineRequest">The data for creating the machine.</param>
    /// <returns>The newly added machine.</returns>
    [HttpPost("createMachine")]
    [ProducesResponseType(typeof(ApiResponse<MachineDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [SwaggerOperation(Summary = "Adds a new machine.", Description = "Registers a new machine with the provided details.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdmin)]
    public async Task<IActionResult> AddAsync([FromBody] AddMachineDto addMachineRequest)
    {
        if (addMachineRequest == null)
            return BadRequest(new ApiResponse<string>(StatusCodes.Status400BadRequest, "Request body cannot be null"));

        var response = await machineService.AddAsync(addMachineRequest);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Updates an existing machine by its ID.
    /// </summary>
    /// <param name="id">The ID of the machine to be updated.</param>
    /// <param name="updateMachineRequest">The data for updating the machine.</param>
    /// <returns>The updated machine.</returns>
    [HttpPut("updateMachine")]
    [ProducesResponseType(typeof(ApiResponse<MachineDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Updates an existing machine.", 
        Description = "Updates the details of an existing machine by its ID.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrTechnician)]
    public async Task<IActionResult> UpdateAsync([FromBody] UpdateMachineDto updateMachineRequest)
    {
        if (updateMachineRequest == null)
            return BadRequest(new ApiResponse<string>(
                StatusCodes.Status400BadRequest, 
                "Invalid request. Machine ID is required."));

        var response = await machineService.UpdateAsync(updateMachineRequest.Id, updateMachineRequest);
        return StatusCode(response.StatusCode, response);   
    }

    /// <summary>
    /// Deletes a machine by its ID.
    /// </summary>
    /// <param name="id">The ID of the machine to be deleted.</param>
    /// <returns>A 204 No Content response if successful.</returns>
    [HttpDelete("deleteMachine/{id:Guid}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Deletes a machine.", Description = "Removes a machine from the system by its ID.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdmin)]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var response = await machineService.DeleteAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    #endregion
}