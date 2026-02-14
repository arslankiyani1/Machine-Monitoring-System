using MMS.Application.Ports.In.CustomerReportSetting.Dto;

namespace MMS.Adapters.Rest.Controllers;

[Route("api/[controller]")]
[ApiController]
//[Authorize(Roles = ApplicationRoles.User)]
public class CustomerReportSettingController(ICustomerReportSettingService customerReportService) : BaseController
{
    #region Queries

    /// <summary>
    /// Retrieves a list of all customer reports.
    /// </summary>
    [HttpGet("getAll")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CustomerReportSettingDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Retrieves all customer reports", Description = "Fetches the complete list of customer reports.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewerOrTechnicianOrOperator)]

    public async Task<IActionResult> GetListAsync(
    [FromQuery] PageParameters pageParameters,
    [FromQuery] ReportFrequency? reportFrequency,
    [FromQuery] bool? isActive,
    [FromQuery] bool? isCustomReport,
    [FromQuery] Guid? customerId,
    [FromQuery] ReportType? reportType)
    {
        var response = await customerReportService.GetListAsync(pageParameters, reportType, isActive, reportFrequency, isCustomReport, customerId);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Retrieves a specific customer report by ID.
    /// </summary>
    [HttpGet("getById/{id:Guid}")]
    [ProducesResponseType(typeof(ApiResponse<CustomerReportSettingDto>), StatusCodes.Status200OK)]
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
    /// Generates and returns a customer report as a file download, and sends it as an email attachment to specified recipients.
    /// </summary>
    [HttpPost("create")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Generates a customer report", Description = "Generates a customer report and returns it as a file download (PDF/CSV/Excel). Also sends the report as an email attachment to specified recipients.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdmin)]
    public async Task<IActionResult> AddAsync([FromBody] AddCustomerReportSettingDto addCustomerReportRequest)
    {
        if (addCustomerReportRequest == null)
        {
            return BadRequest(new ApiResponse<string>(StatusCodes.Status400BadRequest, "Request body cannot be null"));
        }

        var response = await customerReportService.AddAsync(addCustomerReportRequest);

        if (response.StatusCode != StatusCodes.Status201Created || response.Data == null)
        {
            return StatusCode(response.StatusCode, response);
        }

        if (response.Data.FileBytes != null && response.Data.FileBytes.Length > 0)
        {
            return File(response.Data.FileBytes, response.Data.ContentType, response.Data.FileName);
        }

        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Updates an existing customer report.
    /// </summary>
    [HttpPut("update")]
    [ProducesResponseType(typeof(ApiResponse<CustomerReportSettingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [SwaggerOperation(Summary = "Updates an existing customer report", Description = "Modifies the details of an existing customer report.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdmin)]
    public async Task<IActionResult> UpdateAsync([FromBody] UpdateCustomerReportSettingDto updateCustomerReportRequest)
    {
        var response = await customerReportService.UpdateAsync(updateCustomerReportRequest.Id, updateCustomerReportRequest);
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