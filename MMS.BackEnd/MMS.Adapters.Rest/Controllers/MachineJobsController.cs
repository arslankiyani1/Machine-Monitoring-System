using MMS.Application.Ports.In.NoSql.MachineJob.Dto;
using Stripe;

namespace MMS.Adapters.Rest.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MachineJobsController(IMachineJobService service) : ControllerBase
{
    #region Queries
    /// <summary>
    /// Retrieves all machine jobs.
    /// </summary>
    [HttpGet("getAllJobs")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<MachineJob>>), StatusCodes.Status200OK)]
    [SwaggerOperation(Summary = "Get all machine jobs", Description = "Fetches all machine jobs.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewerOrTechnicianOrOperator)]
    public async Task<IActionResult> GetListAsync([FromQuery] PageParameters pageParameters,
        [FromQuery] Guid? machineId, [FromQuery] Guid? CustomerId)
    {
        var response = await service.GetAllAsync(pageParameters,machineId, CustomerId);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Retrieves a machine job by ID.
    /// </summary>
    [HttpGet("getById/{id}")]
    [ProducesResponseType(typeof(ApiResponse<MachineJob>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Get job by ID", Description = "Fetches a machine job using its unique ID.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewerOrTechnicianOrOperator)]

    public async Task<IActionResult> GetByIdAsync(string id)
    {
        var response = await service.GetByIdAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Retrieves detailed stats for a machine job by ID.
    /// </summary>
    [HttpGet("getJobDetailsStats/{jobId}")]
    [ProducesResponseType(typeof(ApiResponse<JobDetailsStats>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Get job details stats", Description = "Fetches OEE, performance, availability, quality, downtime, and utilization stats for a specific machine job.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewerOrTechnicianOrOperator)]
    public async Task<IActionResult> GetJobDetailsStatsAsync(string jobId)
    {
        var response = await service.GetJobDetailsStatsAsync(jobId);
        return StatusCode(response.StatusCode, response);
    }

    #endregion

    /// <summary>
    /// Retrieves a summary of machine jobs by customer.
    /// </summary>
    [HttpGet("{customerId}")]
    [ProducesResponseType(typeof(ApiResponse<JobStatusSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Get job summary", Description = "Fetches job status counts for a given customer.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewerOrTechnicianOrOperator)]
    public async Task<IActionResult> GetJobSummary([FromQuery] PageParameters pageParameters,Guid customerId)
    {
        var response = await service.GetJobSummaryAsync(pageParameters, customerId);
        return StatusCode(response.StatusCode, response);
    }

    #region Commands
    /// <summary>
    /// Creates a new machine job.
    /// </summary>
    [HttpPost("createJob")]
    [ProducesResponseType(typeof(ApiResponse<MachineJob>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [SwaggerOperation(Summary = "Create new machine job", Description = "Adds a new machine job.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrOperator)]
    public async Task<IActionResult> AddAsync([FromBody] MachineJobAddDto request)
    {
        if (request == null)
            return BadRequest(new ApiResponse<string>(StatusCodes.Status400BadRequest, "Request body cannot be null"));

        var response = await service.CreateAsync(request);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Updates a machine job.
    /// </summary>
    [HttpPut("updateJob")]
    [ProducesResponseType(typeof(ApiResponse<MachineJob>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Update machine job", Description = "Updates an existing machine job.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrTechnicianOrOperator)]
    public async Task<IActionResult> UpdateAsync([FromBody] MachineJobUpdateDto request)
    {
        if (request == null)
            return BadRequest(new ApiResponse<string>(StatusCodes.Status400BadRequest, "Invalid request. Job ID is required."));

        var response = await service.UpdateAsync(request);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Deletes a machine job by ID.
    /// </summary>
    [HttpDelete("deleteJob/{id}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Delete machine job", Description = "Deletes a machine job by ID.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdmin)]
    public async Task<IActionResult> DeleteAsync(string id)
    {
        var response = await service.DeleteAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    #endregion
}
