namespace MMS.Adapters.Rest.Controllers;

[Route("api/[controller]")]
[ApiController]
// [Authorize(Roles = ApplicationRoles.User)]
public class MachineMaintenanceTaskController(IMachineMaintenanceTaskService taskService) : BaseController
{
    #region Queries

    /// <summary>
    /// Retrieves a list of all machine maintenance tasks.
    /// </summary>
    [HttpGet("getAllTasks")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<MachineMaintenanceTaskDto>>), StatusCodes.Status200OK)]
    [SwaggerOperation(Summary = "Retrieves a list of all maintenance tasks.", Description = "Fetches all machine maintenance tasks available in the system.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewerOrTechnicianOrOperator)]
    public async Task<IActionResult> GetListAsync([FromQuery] PageParameters pageParameters, [FromQuery] Guid? machineId)
    {
        var response = await taskService.GetListAsync(pageParameters,machineId);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Retrieves a machine maintenance task by ID.
    /// </summary>
    [HttpGet("getById/{id:Guid}")]
    [ProducesResponseType(typeof(ApiResponse<MachineMaintenanceTaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Retrieves a maintenance task by ID.", Description = "Fetches a specific machine maintenance task by its unique ID.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewerOrTechnicianOrOperator)]
    public async Task<IActionResult> GetByIdAsync(Guid id)
    {
        var response = await taskService.GetByIdAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    #endregion

    #region Commands

    /// <summary>
    /// Adds a new machine maintenance task.
    /// </summary>
    [HttpPost("createTask")]
    [ProducesResponseType(typeof(ApiResponse<MachineMaintenanceTaskDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [SwaggerOperation(Summary = "Adds a new maintenance task.", Description = "Creates a new maintenance task for a machine.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrTechnicianOrOperator)]
    public async Task<IActionResult> AddAsync([FromForm] AddMachineMaintenanceTaskDto addTaskRequest)
    {
        if (addTaskRequest == null)
            return BadRequest(new ApiResponse<string>(StatusCodes.Status400BadRequest, "Request body cannot be null"));

        var response = await taskService.AddAsync(addTaskRequest);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Updates an existing maintenance task by its ID.
    /// </summary>
    [HttpPut("updateTask")]
    [ProducesResponseType(typeof(ApiResponse<MachineMaintenanceTaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Updates an existing maintenance task.", Description = "Updates the details of an existing machine maintenance task.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrTechnicianOrOperator)]
    public async Task<IActionResult> UpdateAsync([FromForm] UpdateMachineMaintenanceTaskDto updateTaskRequest)
    {
        if (updateTaskRequest == null)
            return BadRequest(new ApiResponse<string>(StatusCodes.Status400BadRequest, "Invalid request. Task ID is required."));

        var response = await taskService.UpdateAsync(updateTaskRequest.Id, updateTaskRequest);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Deletes a maintenance task by its ID.
    /// </summary>
    [HttpDelete("deleteTask/{id:Guid}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Deletes a maintenance task.", Description = "Removes a machine maintenance task from the system by its ID.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrTechnician)]

    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var response = await taskService.DeleteAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    #endregion
}