namespace MMS.Adapters.Rest.Controllers;

[Route("api/[controller]")]
[ApiController]
// [Authorize(Roles = ApplicationRoles.User)] // Uncomment if role-based access is needed
public class InvoiceController(IInvoiceService invoiceService) : BaseController
{
    #region Queries

    /// <summary>
    /// Retrieves all invoices.
    /// </summary>
    /// <param name="pageParameters">Pagination parameters</param>
    /// <returns>List of invoices</returns>
    [HttpGet("getAllInvoices")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<InvoiceDto>>), StatusCodes.Status200OK)]
    [SwaggerOperation(Summary = "Retrieves all invoices", Description = "Fetches a paginated list of all invoices.")]
    //[Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewer)]
    public async Task<IActionResult> GetListAsync([FromQuery] PageParameters pageParameters)
    {
        var response = await invoiceService.GetListAsync(pageParameters);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Retrieves an invoice by ID.
    /// </summary>
    /// <param name="id">Invoice ID</param>
    /// <returns>Invoice details</returns>
    [HttpGet("getById/{id:Guid}")]
    [ProducesResponseType(typeof(ApiResponse<InvoiceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Gets invoice by ID", Description = "Fetches the invoice details for the specified ID.")]
    //[Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewer)]
    public async Task<IActionResult> GetByIdAsync(Guid id)
    {
        var response = await invoiceService.GetByIdAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    #endregion

    #region Commands

    /// <summary>
    /// Creates a new invoice.
    /// </summary>
    /// <param name="addInvoiceRequest">Invoice data</param>
    /// <returns>Created invoice</returns>
    [HttpPost("createInvoice")]
    [ProducesResponseType(typeof(ApiResponse<InvoiceDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [SwaggerOperation(Summary = "Creates an invoice", Description = "Adds a new invoice to the system.")]
    //[Authorize(Policy = AuthorizationPolicies.RequireSystemAdmin)]
    public async Task<IActionResult> AddAsync([FromForm] AddInvoiceDto addInvoiceRequest)
    {
        if (addInvoiceRequest == null)
        {
            return BadRequest(new ApiResponse<string>(StatusCodes.Status400BadRequest, "Request body cannot be null"));
        }

        var response = await invoiceService.AddAsync(addInvoiceRequest);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Updates an existing invoice.
    /// </summary>
    /// <param name="updateInvoiceRequest">Invoice update data</param>
    /// <returns>Updated invoice</returns>
    [HttpPut("updateInvoice")]
    [ProducesResponseType(typeof(ApiResponse<InvoiceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [SwaggerOperation(Summary = "Updates an invoice", Description = "Updates the details of an existing invoice.")]
    //[Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdmin)]
    public async Task<IActionResult> UpdateAsync([FromForm] UpdateInvoiceDto updateInvoiceRequest)
    {
        var response = await invoiceService.UpdateAsync(updateInvoiceRequest.Id, updateInvoiceRequest);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Deletes an invoice by ID.
    /// </summary>
    /// <param name="id">Invoice ID</param>
    /// <returns>Success message</returns>
    [HttpDelete("deleteInvoice/{id:Guid}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Deletes an invoice", Description = "Removes an invoice by its ID.")]
    //[Authorize(Policy = AuthorizationPolicies.RequireSystemAdmin)]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var response = await invoiceService.DeleteAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    #endregion
}
