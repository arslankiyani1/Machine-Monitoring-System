namespace MMS.Adapters.Rest.Controllers;

[ApiController]
[Route("api/[controller]")]

public class MachineSensorController(IMachineSensorService machineSensorService) : BaseController
{
    #region Queries

    /// <summary>
    /// Retrieves all sensors associated with a specific customer.
    /// </summary>
    [HttpGet("customer/{customerId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<MachineSensorDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(
        Summary = "Get sensors by customer ID",
        Description = "Fetches all machine sensors linked to the specified customer ID."
    )]
   [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewerOrTechnicianOrOperatorOrMMSBridge)]
    public async Task<IActionResult> GetSensorsByCustomerIdAsync(Guid customerId, [FromQuery] PageParameters pageParameters)
    {
        var response = await machineSensorService.GetSensorsByCustomerIdAsync(customerId, pageParameters);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Retrieves a list of all machine sensors.
    /// </summary>
    [HttpGet("list")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<MachineSensorDto>>), StatusCodes.Status200OK)]
    [SwaggerOperation(
        Summary = "Get all machine sensors",
        Description = "Returns a paginated list of all machine sensors."
    )]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewerOrTechnicianOrOperatorOrMMSBridge)]
    public async Task<IActionResult> GetAllAsync([FromQuery] PageParameters pageParameters)
    {
        var response = await machineSensorService.GetListAsync(pageParameters);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Retrieves a machine sensor by its ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<MachineSensorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(
        Summary = "Get machine sensor by ID",
        Description = "Returns the machine sensor details for the given ID."
    )]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrTechnicianOrOperator)]
    public async Task<IActionResult> GetByIdAsync(Guid id)
    {
        var response = await machineSensorService.GetByIdAsync(id);
        return StatusCode(response.StatusCode, response);
    }
    #endregion

    #region Commands

    /// <summary>
    /// Creates a new machine sensor.
    /// </summary>
    [HttpPost("create")]
    [ProducesResponseType(typeof(ApiResponse<MachineSensorDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [SwaggerOperation(
        Summary = "Create a new machine sensor",
        Description = "Adds a new machine sensor record to the system."
    )]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrTechnicianOrOperator)]
    public async Task<IActionResult> AddAsync([FromBody] AddMachineSensorDto request)
    {
        if (request is null)
            return BadRequest(new ApiResponse<string>(StatusCodes.Status400BadRequest, "Request body cannot be null"));

        var response = await machineSensorService.AddAsync(request);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Updates an existing machine sensor.
    /// </summary>
    [HttpPut]
    [ProducesResponseType(typeof(ApiResponse<MachineSensorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(
        Summary = "Update machine sensor by ID",
        Description = "Updates the details of an existing machine sensor."
    )]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrTechnicianOrOperator)]
    public async Task<IActionResult> UpdateAsync([FromBody] UpdateMachineSensorDto request)
    {
        var response = await machineSensorService.UpdateAsync(request);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Deletes a machine sensor by its ID.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(
        Summary = "Delete machine sensor by ID",
        Description = "Deletes the machine sensor record if it exists."
    )]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrTechnicianOrOperator)]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var response = await machineSensorService.DeleteAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    #endregion
}
