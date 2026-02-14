using MMS.Application.Ports.In.NoSql.MachineLog.Dto;

namespace MMS.Adapters.Rest.Controllers;

/// <summary>
/// Controller for managing machine logs and performance data like utilization, downtime, and spindle usage.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class MachineLogsController(IMachineLogService service) : ControllerBase
{
    #region Queries

    /// <summary>
    /// Retrieves machine utilization statistics.
    /// </summary>
    /// <param name="machineId">The machine ID.</param>
    /// <param name="range">Predefined time range (e.g., Daily, Weekly).</param>
    /// <param name="from">Custom start time (optional).</param>
    /// <param name="to">Custom end time (optional).</param>
    /// <returns>Utilization data over the specified time range.</returns>
    [HttpGet("utilization/{machineId}")]
    [SwaggerOperation(Summary = "Get machine utilization", Description = "Returns utilization metrics for a machine.")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewerOrTechnicianOrOperatorOrMMSBridge)]
    public async Task<IActionResult> GetUtilization(Guid machineId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var result = await service.GetMachineUtilizationAsync(machineId, from, to);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Retrieves machine downtime statistics.
    /// </summary>
    /// <param name="machineId">The machine ID.</param>
    /// <param name="range">Predefined time range.</param>
    /// <param name="jobId">Optional: Filter by specific job ID.</param>
    /// <param name="from">Custom start time (optional).</param>
    /// <param name="to">Custom end time (optional).</param>
    /// <returns>Downtime data over the specified time range.</returns>
    [HttpGet("downtime/{machineId}")]
    [SwaggerOperation(
        Summary = "Get machine downtime",
        Description = "Returns downtime metrics for a machine, optionally filtered by a specific job."
    )]
    [ProducesResponseType(typeof(ApiResponse<DowntimeApiResponseDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewerOrTechnicianOrOperatorOrMMSBridge)]
    public async Task<IActionResult> GetDowntime(
        Guid machineId,
        [FromQuery] Guid? jobId = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        // Call the service with named arguments to avoid parameter mismatch
        var result = await service.GetMachineDowntimeAsync( machineId: machineId, from: from, to: to,jobId: jobId);
        return StatusCode(result.StatusCode, result);
    }


    /// <summary>
    /// Retrieves the downtime timeline for a specific machine within a given time range.
    /// </summary>
    /// <param name="machineId">The unique identifier of the machine.</param>
    /// <param name="range">A predefined time range (e.g., Today, Last7Days, Last30Days, Custom).</param>
    /// <param name="from">The start date and time when using a custom range (optional).</param>
    /// <param name="to">The end date and time when using a custom range (optional).</param>
    /// <returns>A timeline of machine downtime events and statistics for the specified range.</returns>
    [HttpGet("activity-timeline/{machineId}")]
    [SwaggerOperation(
        Summary = "Get machine downtime timeline",
        Description = "Retrieves downtime metrics and events for a machine over a specified time range. "
                    + "Supports both predefined ranges (e.g., Today, Last7Days) and custom date ranges using 'from' and 'to' parameters."
    )]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewerOrTechnicianOrOperatorOrMMSBridge)]
    public async Task<IActionResult> GetMachineTimeline(Guid machineId,[FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var result = await service.GetMachineTimelineAsync(machineId, from, to);
        return StatusCode(result.StatusCode, result);
    }


    /// <summary>
    /// Retrieves machine metrics for a specified machine and time range.
    /// </summary>
    /// <param name="machineId">The machine ID.</param>
    /// <param name="metric">The metric to retrieve (SpindleSpeed or FeedRate).</param>
    /// <param name="range">Predefined time range (Daily, Weekly, Monthly).</param>
    /// <param name="from">Custom start time.</param>
    /// <param name="to">Custom end time.</param>
    /// <returns>Metrics data for the specified time range.</returns>
    [HttpGet("metrics/{machineId}")]
    [SwaggerOperation(Summary = "Get machine metrics", 
        Description = "Returns time-series metrics data (SpindleSpeed or FeedRate) for a machine.")]
    [ProducesResponseType(typeof(ApiResponse<MetricResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<MetricResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<MetricResponseDto>), StatusCodes.Status500InternalServerError)]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewerOrTechnicianOrOperatorOrMMSBridge)]
    public async Task<IActionResult> GetMetricsAsync(
        [FromRoute] Guid machineId,
        [FromQuery] MetricType metric,
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] TimeRange range)
    {
        var request = new MetricRequestDto
        {
            Metric = metric,
            From = from,
            To = to,
            Range = range
        };

        var response = await service.GetMetricsAsync(machineId, request);
        return StatusCode(response.StatusCode, response);
    }


    /// <summary>
    /// Gets all machine logs.
    /// </summary>
    /// <returns>List of all machine logs.</returns>
    [HttpGet("GetAll")]
    [SwaggerOperation(Summary = "Get all machine logs", Description = "Returns all machine logs from the database.")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<MachineLog>>), StatusCodes.Status200OK)]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewerOrTechnicianOrOperatorOrMMSBridge)]
    public async Task<IActionResult> GetAll([FromQuery] PageParameters pageParameters)
    {
        var response = await service.GetAllAsync(pageParameters);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Retrieves a machine log by its ID.
    /// </summary>
    /// <param name="id">The ID of the machine log.</param>
    /// <returns>The requested machine log.</returns>
    [HttpGet("GetById/{id}")]
    [SwaggerOperation(Summary = "Get machine log by ID", Description = "Fetches a machine log using its ID.")]
    [ProducesResponseType(typeof(MachineLog), StatusCodes.Status200OK)]
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
    /// Creates a new machine log entry.
    /// </summary>
    /// <param name="dto">The machine log data to create.</param>
    /// <returns>The newly created machine log.</returns>
    [HttpPost("Create")]
    [SwaggerOperation(Summary = "Create machine log", Description = "Adds a new machine log record.")]
    [ProducesResponseType(typeof(MachineLog), StatusCodes.Status201Created)]
   // [Authorize(Policy = AuthorizationPolicies.RequireMMSBridge)]
    public async Task<IActionResult> Create([FromBody] MachineLog model)
    {
        var response = await service.CreateAsync(model);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Updates an existing machine log.
    /// </summary>
    /// <param name="dto">The machine log object with updated data.</param>
    [HttpPut("Update")]
    [SwaggerOperation(Summary = "Update machine log", Description = "Updates an existing machine log.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [Authorize(Policy = AuthorizationPolicies.RequireMMSBridge)]
    public async Task<IActionResult> Update([FromBody] MachineLog model)
    {
        var response = await service.UpdateAsync(model);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Deletes a machine log by ID.
    /// </summary>
    /// <param name="id">The ID of the machine log to delete.</param>
    [HttpDelete("Delete/{id}")]
    [SwaggerOperation(Summary = "Delete machine log", Description = "Deletes a machine log entry by its ID.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [Authorize(Policy = AuthorizationPolicies.DenyAll)]
    public async Task<IActionResult> Delete(string id)
    {
        var response = await service.DeleteAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    #endregion
}
