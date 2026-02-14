using MMS.Application.Ports.In.NoSql.CustomerDashSummary;

namespace MMS.Adapters.Rest.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomerDashboardSummariesController : BaseController
{
    private readonly ICustomerDashboardSummaryService service;

    public CustomerDashboardSummariesController(ICustomerDashboardSummaryService service)
    {
        this.service = service;
    }

    #region Queries

    /// <summary>
    /// Retrieves all customer dashboard summaries.
    /// </summary>
    [HttpGet("getAll")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CustomerDashboardSummary>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Get all customer dashboard summaries", Description = "Returns a list of all customer dashboard summaries.")]
    //[Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewerOrTechnicianOrOperator)]
    public async Task<IActionResult> GetAllAsync()
    {
        var response = await service.GetAllAsync();
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Retrieves a customer dashboard summary by customer ID.
    /// </summary>
    [HttpGet("getByCustomerId")]
    [ProducesResponseType(typeof(ApiResponse<CustomerDashboardSummary>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Get customer dashboard summary by customer ID", Description = "Returns a customer dashboard summary for a given customer ID.")]
    //[Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewerOrTechnicianOrOperator)]
    public async Task<IActionResult> GetByCustomerIdAsync([FromQuery] string customerId)
    {
        var response = await service.GetByCustomerIdAsync(customerId);
        return StatusCode(response.StatusCode, response);
    }

    #endregion

    #region Commands

    /// <summary>
    /// Upserts a customer dashboard summary.
    /// </summary>
    [HttpPut("upsert")]
    [ProducesResponseType(typeof(ApiResponse<CustomerDashboardSummary>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [SwaggerOperation(Summary = "Upsert customer dashboard summary", Description = "Creates or overwrites a customer dashboard summary based on customer ID.")]
   // [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrTechnician)]
    public async Task<IActionResult> UpsertAsync([FromBody] CustomerDashboardSummary model)
    {
        if (model == null)
            return BadRequest(new ApiResponse<string>(StatusCodes.Status400BadRequest, "Request body cannot be null"));

        var response = await service.UpsertAsync(model);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Deletes a customer dashboard summary by customer ID.
    /// </summary>
    [HttpDelete("delete/{customerId}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Delete customer dashboard summary", Description = "Deletes a customer dashboard summary by customer ID.")]
   // [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdmin)]
    public async Task<IActionResult> DeleteAsync(string customerId)
    {
        var response = await service.DeleteAsync(customerId);
        return StatusCode(response.StatusCode, response);
    }

    #endregion
}