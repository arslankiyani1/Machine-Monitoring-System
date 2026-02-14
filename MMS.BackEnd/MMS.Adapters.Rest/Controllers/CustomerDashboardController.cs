namespace MMS.Adapters.Rest.Controllers;

[Route("api/[controller]")]
[ApiController]
// [Authorize(Roles = ApplicationRoles.User)]
public class CustomerDashboardController(ICustomerDashboardService customerDashboardService) : BaseController
{
    #region Queries

    /// <summary>
    /// Retrieves a list of all customer dashboards.
    /// </summary>
    /// <returns>A list of all customer dashboards.</returns>
    [HttpGet("getAllCustomerDashboards")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CustomerDashboardDto>>), StatusCodes.Status200OK)]
    [SwaggerOperation(Summary = "Retrieves a list of all customer dashboards.", Description = "Fetches all customer dashboards available in the system.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewerOrTechnicianOrOperator)]
    public async Task<IActionResult> GetListAsync([FromQuery] PageParameters pageParameters,[FromQuery] DashboardStatus? dashboardStatus,Guid CustomerId)
    {
        var response = await customerDashboardService.GetListAsync(pageParameters,dashboardStatus,CustomerId);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Retrieves a customer dashboard by ID.
    /// </summary>
    [HttpGet("getById/{id:Guid}")]
    [ProducesResponseType(typeof(ApiResponse<CustomerDashboardDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Retrieves a customer dashboard by ID.", Description = "Fetches a specific customer dashboard in the system by its unique ID.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewerOrTechnicianOrOperator)]
    public async Task<IActionResult> GetByIdAsync(Guid id)
    {
        var response = await customerDashboardService.GetByIdAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    #endregion

    #region Commands

    /// <summary>
    /// Adds a new customer dashboard.
    /// </summary>
    /// <param name="addCustomerDashboardRequest">The data for creating the customer dashboard.</param>
    /// <returns>The newly added customer dashboard.</returns>
    [HttpPost("createCustomerDashboard")]
    [ProducesResponseType(typeof(ApiResponse<CustomerDashboardDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [SwaggerOperation(Summary = "Adds a new customer dashboard.", Description = "Registers a new customer dashboard with the provided details.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdmin)]
    public async Task<IActionResult> AddAsync([FromBody] AddCustomerDashboardDto addCustomerDashboardRequest)
    {
        if (addCustomerDashboardRequest == null)
            return BadRequest(new ApiResponse<string>(StatusCodes.Status400BadRequest, "Request body cannot be null"));

        var response = await customerDashboardService.AddAsync(addCustomerDashboardRequest);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Updates an existing customer dashboard by its ID.
    /// </summary>
    /// <param name="updateCustomerDashboardRequest">The data for updating the customer dashboard.</param>
    /// <returns>The updated customer dashboard.</returns>
    [HttpPut("updateCustomerDashboard")]
    [ProducesResponseType(typeof(ApiResponse<CustomerDashboardDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Updates an existing customer dashboard.", Description = "Updates the details of an existing customer dashboard by its ID.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdmin)]

    public async Task<IActionResult> UpdateAsync([FromBody] UpdateCustomerDashboardDto updateCustomerDashboardRequest)
    {
        if (updateCustomerDashboardRequest == null)
            return BadRequest(new ApiResponse<string>(StatusCodes.Status400BadRequest, "Invalid request. Customer Dashboard ID is required."));

        var response = await customerDashboardService.UpdateAsync(updateCustomerDashboardRequest.Id, updateCustomerDashboardRequest);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Deletes a customer dashboard by its ID.
    /// </summary>
    /// <param name="id">The ID of the customer dashboard to be deleted.</param>
    /// <returns>A 204 No Content response if successful.</returns>
    [HttpDelete("deleteCustomerDashboard/{id:Guid}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Deletes a customer dashboard.", Description = "Removes a customer dashboard from the system by its ID.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdmin)]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var response = await customerDashboardService.DeleteAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    #endregion
}