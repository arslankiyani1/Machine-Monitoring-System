using MMS.Application.Ports.In.CustomerBilling;
using MMS.Application.Ports.In.CustomerBilling.Dto;

namespace MMS.Adapters.Rest.Controllers;

[Route("api/[controller]")]
[ApiController]
// [Authorize(Roles = ApplicationRoles.User)]
public class CustomerBillingAddressController(ICustomerBillingAddressService service) : BaseController
{
    /// <summary>
    /// Get all billing addresses for a customer.
    /// </summary>
    [HttpGet("getAll/{customerId:Guid}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CustomerBillingAddressDto>>), StatusCodes.Status200OK)]
    [SwaggerOperation(Summary = "Get billing addresses", Description = "Get all billing addresses for a specific customer.")]
    //[Authorize(Policy = AuthorizationPolicies.RequireSystemAdmin)]
    public async Task<IActionResult> GetListAsync(Guid customerId)
    {
        var response = await service.GetListAsync(customerId);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Get billing address by ID.
    /// </summary>
    [HttpGet("getById/{billingAddressId:Guid}")]
    [ProducesResponseType(typeof(ApiResponse<CustomerBillingAddressDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Get billing address by ID", Description = "Retrieve billing address by its unique ID.")]
    //[Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdmin)]
    public async Task<IActionResult> GetByIdAsync(Guid billingAddressId)
    {
        var response = await service.GetByIdAsync(billingAddressId);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Add a new billing address.
    /// </summary>
    [HttpPost("create")]
    [ProducesResponseType(typeof(ApiResponse<CustomerBillingAddressDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [SwaggerOperation(Summary = "Add billing address", Description = "Create a new billing address for a customer.")]
    //[Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdmin)]
    public async Task<IActionResult> AddAsync([FromForm] AddCustomerBillingAddressDto request)
    {
        if (request == null)
            return BadRequest(new ApiResponse<string>(StatusCodes.Status400BadRequest, "Request body cannot be null"));

        var response = await service.AddAsync(request);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Update a billing address.
    /// </summary>
    [HttpPut("update")]
    [ProducesResponseType(typeof(ApiResponse<CustomerBillingAddressDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Update billing address", Description = "Update an existing billing address.")]
    //[Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdmin)]
    public async Task<IActionResult> UpdateAsync([FromForm] UpdateCustomerBillingAddressDto request)
    {
        var response = await service.UpdateAsync(request.Id, request);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Delete a billing address by ID.
    /// </summary>
    [HttpDelete("delete/{billingAddressId:Guid}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Delete billing address", Description = "Delete a billing address by ID.")]
    //[Authorize(Policy = AuthorizationPolicies.RequireSystemAdmin)]
    public async Task<IActionResult> DeleteAsync(Guid billingAddressId)
    {
        var response = await service.DeleteAsync(billingAddressId);
        return StatusCode(response.StatusCode, response);
    }
}
