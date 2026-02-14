namespace MMS.Adapters.Rest.Controllers;


[Route("api/[controller]")]
[ApiController]
// [Authorize(Roles = ApplicationRoles.User)]
public class CustomerReportController(ICustomerReportService customerReportService) : BaseController
{
    #region Queries

    /// <summary>
    /// Retrieves a list of all customer reports.
    /// </summary>
    [HttpGet("getAll")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CustomerReportDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Retrieves all customer reports", Description = "Fetches the complete list of customer reports.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewerOrTechnicianOrOperator)]
    public async Task<IActionResult> GetListAsync(
        [FromQuery] PageParameters pageParameters,
        [FromQuery] ReportType? reportType,
        [FromQuery] bool? isSent,
        [FromQuery] ReportFrequency? reportFrequency, [FromQuery] Guid? customerId)
    {
        var response = await customerReportService.GetListAsync(pageParameters, reportType, isSent, reportFrequency,customerId);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Retrieves a specific customer report by ID.
    /// </summary>
    [HttpGet("getById/{id:Guid}")]
    [ProducesResponseType(typeof(ApiResponse<CustomerReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Retrieves a customer report by ID", Description = "Fetches details of a specific customer report using its unique identifier.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewerOrTechnicianOrOperator)]
    public async Task<IActionResult> GetByIdAsync(Guid id)
    {
        var response = await customerReportService.GetByIdAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    #endregion

    #region Commands

    /// <summary>
    /// Adds a new customer report.
    /// </summary>
    [HttpPost("create")]
    [ProducesResponseType(typeof(ApiResponse<CustomerReportDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [SwaggerOperation(Summary = "Creates a new customer report", Description = "Registers a new customer report.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdmin)]
    public async Task<IActionResult> AddAsync([FromBody] AddCustomerReportDto addCustomerReportDto)
    {
        if (addCustomerReportDto == null)
        {
            return BadRequest(new ApiResponse<string>(StatusCodes.Status400BadRequest, "Request body cannot be null"));
        }

        var response = await customerReportService.AddAsync(addCustomerReportDto);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Deletes a customer report by ID.
    /// </summary>
    [HttpDelete("delete/{id:Guid}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Deletes a customer report", Description = "Removes a customer report from the system.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdmin)]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var response = await customerReportService.DeleteAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    #endregion
}
