namespace MMS.Adapters.Rest.Controllers;

[Route("api/[controller]")]
[ApiController]
//[Authorize(Roles = ApplicationRoles.User)]
public class UserMachineController(IUserMachineService userMachineService) : BaseController
{
    #region Queries

    /// <summary>
    /// Retrieves a list of all user machines.
    /// </summary>
    /// <returns>A list of all user machines.</returns>
    [HttpGet("getAllUserMachines")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserMachineDto>>), StatusCodes.Status200OK)]
    [SwaggerOperation(Summary = "Retrieves a list of all user machines.", Description = "Fetches all user machines available in the system.")]
    public async Task<IActionResult> GetListAsync([FromQuery] PageParameters pageParameters)
    {
        var response = await userMachineService.GetListAsync(pageParameters);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Retrieves a user machine by ID.
    /// </summary>
    [HttpGet("getById/{id:Guid}")]
    [ProducesResponseType(typeof(ApiResponse<UserMachineDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Retrieves a user machine by ID.", 
        Description = "Fetches a specific user machine in the system by its unique ID.")]
    public async Task<IActionResult> GetByIdAsync(Guid id)
    {
        var response = await userMachineService.GetByIdAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    #endregion

    #region Commands

    /// <summary>
    /// Adds a new user machine.
    /// </summary>
    /// <param name="addUserMachineRequest">The data for creating the user machine.</param>
    /// <returns>The newly added user machine.</returns>
    [HttpPost("createUserMachine")]
    [ProducesResponseType(typeof(ApiResponse<UserMachineDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [SwaggerOperation(Summary = "Adds a new user machine.", 
        Description = "Registers a new user machine with the provided details.")]
    public async Task<IActionResult> AddAsync([FromBody] AddUserMachineDto addUserMachineRequest)
    {
        if (addUserMachineRequest == null)
            return BadRequest(new ApiResponse<string>(StatusCodes.Status400BadRequest, "Request body cannot be null"));

        var response = await userMachineService.AddAsync(addUserMachineRequest);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Updates an existing user machine by its ID.
    /// </summary>
    /// <param name="updateUserMachineRequest">The data for updating the user machine.</param>
    /// <returns>The updated user machine.</returns>
    [HttpPut("updateUserMachine")]
    [ProducesResponseType(typeof(ApiResponse<UserMachineDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Updates an existing user machine.", 
        Description = "Updates the details of an existing user machine by its ID.")]
    public async Task<IActionResult> UpdateAsync([FromBody] UpdateUserMachineDto updateUserMachineRequest)
    {
        if (updateUserMachineRequest == null)
            return BadRequest(new ApiResponse<string>(StatusCodes.Status400BadRequest, "Invalid request. User Machine ID is required."));

        var response = await userMachineService.UpdateAsync(updateUserMachineRequest.Id, updateUserMachineRequest);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Deletes a user machine by its ID.
    /// </summary>
    /// <param name="id">The ID of the user machine to be deleted.</param>
    /// <returns>A 204 No Content response if successful.</returns>
    [HttpDelete("deleteUserMachine/{id:Guid}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Deletes a user machine.", Description = "Removes a user machine from the system by its ID.")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var response = await userMachineService.DeleteAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    #endregion
}
