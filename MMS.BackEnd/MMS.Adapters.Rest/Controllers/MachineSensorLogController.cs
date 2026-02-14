using MMS.Application.Ports.In.NoSql.MachineSensorLog.Dto;

namespace MMS.Adapters.Rest.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MachineSensorLogController(IMachineSensorLogService service) : ControllerBase
{
    #region Queries

    /// <summary>
    /// Retrieves sensor trend data for all parameters within a selected time range.
    /// </summary>
    /// <param name="sensorId">The sensor ID.</param>
    /// <param name="timeRange">The time range (Daily, Weekly, Monthly).</param>
    /// <returns>Grouped parameter data points for chart visualization.</returns>
    [HttpGet("trend")]
    [SwaggerOperation(Summary = "Get sensor trend data", Description = "Retrieves sensor trend data for all parameters within a selected time range.")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<SensorTrendDataDto>>), StatusCodes.Status200OK)]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewerOrTechnicianOrOperatorOrMMSBridge)]
    public async Task<IActionResult> GetSensorTrendAsync([FromQuery] Guid sensorId, [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var response = await service.GetSensorTrendAsync(sensorId, from, to);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Retrieves the latest sensor data within the given time range.
    /// </summary>
    /// <param name="sensorId">The sensor ID.</param>
    /// <param name="timeRange">The time range.</param>
    /// <returns>Latest sensor readings.</returns>
    [HttpGet("latest")]
    [SwaggerOperation(Summary = "Get latest sensor data", Description = "Retrieves the latest sensor data within the given time range.")]
    [ProducesResponseType(typeof(ApiResponse<MachineSensorLogDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewerOrTechnicianOrOperatorOrMMSBridge)]
    public async Task<IActionResult> GetLatestAsync([FromQuery] Guid sensorId, [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var response = await service.GetLatestSensorDataAsync(sensorId, from, to);
        return StatusCode(response.StatusCode, response);
    }


    /// <summary>
    /// Retrieves all machine sensor data entries.
    /// </summary>
    [HttpGet("GetAllMachineSensorData")]
    [SwaggerOperation(Summary = "Get all machine sensor data", Description = "Fetches all machine sensor data records.")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<MachineSensorLog>>), StatusCodes.Status200OK)]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewerOrTechnicianOrOperatorOrMMSBridge)]
    public async Task<IActionResult> GetAll([FromQuery] PageParameters pageParameters)
    {
        var response = await service.GetAllAsync(pageParameters);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Retrieves a specific machine sensor data entry by ID.
    /// </summary>
    /// <param name="id">The ID of the machine sensor data.</param>
    [HttpGet("GetByIdMachineSensorData/{id}")]
    [SwaggerOperation(Summary = "Get machine sensor data by ID", Description = "Fetches a single machine sensor data entry by its ID.")]
    [ProducesResponseType(typeof(ApiResponse<MachineSensorLog>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewerOrTechnicianOrOperatorOrMMSBridge)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var response = await service.GetByIdAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    #endregion

    #region Commands
    /// <summary>
    /// Creates a new machine sensor data entry.
    /// </summary>
    /// <param name="model">The new machine sensor data to create.</param>
    [HttpPost("CreateMachineSensorData")]
    [SwaggerOperation(Summary = "Create new machine sensor data", Description = "Creates a new machine sensor data entry.")]
    [ProducesResponseType(typeof(ApiResponse<MachineSensorLog>), StatusCodes.Status201Created)]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewerOrTechnicianOrOperatorOrMMSBridge)]
    public async Task<IActionResult> Create([FromBody] AddMachineSensorLogDto model)
    {
        var response = await service.CreateAsync(model);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Updates an existing machine sensor data entry.
    /// </summary>
    /// <param name="id">The ID of the machine sensor data to update.</param>
    /// <param name="model">Updated data.</param>
    [HttpPut("UpdateMachineSensorData")]
    [SwaggerOperation(Summary = "Update machine sensor data", Description = "Updates a machine sensor data entry by ID.")]
    [ProducesResponseType(typeof(ApiResponse<MachineSensorLog>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewerOrTechnicianOrOperatorOrMMSBridge)]
    public async Task<IActionResult> Update([FromBody] UpdateMachineSensorLogDto model)
    {
        var response = await service.UpdateAsync(model);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Deletes a machine sensor data entry.
    /// </summary>
    /// <param name="id">The ID of the machine sensor data to delete.</param>
    [HttpDelete("DeleteMachineSensorData/{id}")]
    [SwaggerOperation(Summary = "Delete machine sensor data", Description = "Deletes a machine sensor data entry by ID.")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewerOrTechnicianOrOperatorOrMMSBridge)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var response = await service.DeleteAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    #endregion
}
