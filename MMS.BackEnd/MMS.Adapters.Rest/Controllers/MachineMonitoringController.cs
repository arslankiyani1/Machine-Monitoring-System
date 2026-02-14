using MMS.Application.Ports.In.NoSql.MachineSensorLog.Dto;
using MMS.Application.Ports.In.NoSql.MachinMonitoring.Dto;

namespace MMS.Adapters.Rest.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MachineMonitoringController (IMachineMonitoringService _monitoringService) : ControllerBase
{
    /// <summary>
    /// Processes machine monitoring signals.
    /// </summary>
    /// <param name="dto">The machine monitoring data.</param>
    /// <returns>A response indicating whether the monitoring data was processed successfully.</returns>
    /// <response code="200">Successfully processed the monitoring signal.</response>
    /// <response code="400">Invalid data or processing failed.</response>
    [HttpPost("machine-logs")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [SwaggerOperation(
        Summary = "Monitor machine signal",
        Description = "Receives and processes a machine monitoring signal for real-time tracking or analytics."
    )]
   [Authorize(Policy = AuthorizationPolicies.RequireMMSBridge)]
    public async Task<IActionResult> SendMachineLog([FromBody] MachineMonitoring dto)
    {
        if (dto == null)
            return BadRequest(new ApiResponse<string>(StatusCodes.Status400BadRequest, "Monitoring data is required."));
        
        var result = await _monitoringService.ProcessMonitoringAsync(dto);
        return Ok(result);
    }

    /// <summary>
    /// Receives and processes machine sensor log data.
    /// </summary>
    /// <param name="dto">The machine sensor log data.</param>
    /// <returns>A response indicating whether the sensor log was processed successfully.</returns>
    /// <response code="200">Successfully processed the sensor log.</response>
    /// <response code="400">Invalid data or processing failed.</response>
    [HttpPost("machine-sensor-logs")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [SwaggerOperation(
        Summary = "Log machine sensor data",
        Description = "Receives and processes machine sensor log data for storage or analytics."
    )]
    [Authorize(Policy = AuthorizationPolicies.RequireMMSBridge)]
    public async Task<IActionResult> LogSensorData([FromBody] MachinemonitoringDto dto)
    {
        if (dto == null)
            return BadRequest(new ApiResponse<string>(StatusCodes.Status400BadRequest, "Sensor log data is required."));

        var result = await _monitoringService.ProcessSensorLogAsync(dto);
        return Ok(result);
    }


    /// <summary>
    /// Receives and processes machine operational data.
    /// </summary>
    /// <param name="dto">The operational data payload including machineId, customerId, and sensor readings.</param>
    /// <returns>A response indicating whether the operational data was processed successfully.</returns>
    /// <response code="200">Successfully processed the operational data.</response>
    /// <response code="400">Invalid data or processing failed.</response>
    [HttpPost("operationaldata")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [SwaggerOperation(
        Summary = "Log machine operational data",
        Description = "Receives and processes machine operational data for storage or analytics."
    )]
    [Authorize(Policy = AuthorizationPolicies.RequireMMSBridge)]
    public async Task<IActionResult> LogOperationalData([FromBody] CreateOperationalData dto)
    {
        if (dto == null)
            return BadRequest(new ApiResponse<string>(StatusCodes.Status400BadRequest, "Operational data is required."));

        var result = await _monitoringService.ProcessOperationalDataAsync(dto);
        return Ok(result);
    }
}