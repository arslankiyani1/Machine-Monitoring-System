namespace MMS.Adapters.Rest.Controllers;

[Route("api/[controller]")]
[ApiController]
// [Authorize(Roles = ApplicationRoles.User)]
public class CustomerSubscriptionController(ICustomerSubscriptionService customerSubscriptionService) : BaseController
{
    #region Queries

    /// <summary>
    /// Retrieves customer subscriptions by customer ID.
    /// </summary>
    [HttpGet("getByCustomerId/{customerId:Guid}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CustomerSubscriptionDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Retrieves customer subscriptions by customer ID.", Description = "Fetches all customer subscriptions associated with a specific customer.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewer)]
    public async Task<IActionResult> GetByCustomerIdAsync(Guid customerId)
    {
        var response = await customerSubscriptionService.GetByCustomerIdAsync(customerId);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Retrieves a list of all customer subscriptions.
    /// </summary>
    /// <returns>A list of all customer subscriptions.</returns>
    [HttpGet("getAllCustomerSubscriptions")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CustomerSubscriptionDto>>), StatusCodes.Status200OK)]
    [SwaggerOperation(Summary = "Retrieves a list of all customer subscriptions.", Description = "Fetches all customer subscriptions available in the system.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewer)]
    public async Task<IActionResult> GetListAsync([FromQuery] PageParameters pageParameters, 
        [FromQuery] Guid customerId,[FromQuery] CustomerSubscriptionStatus? status, 
        [FromQuery] DateTime? start, [FromQuery] DateTime? End)
    {
        var response = await customerSubscriptionService.GetListAsync(pageParameters, customerId, status,start,End);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Retrieves a customer subscription by ID.
    /// </summary>
    [HttpGet("getById/{id:Guid}")]
    [ProducesResponseType(typeof(ApiResponse<CustomerSubscriptionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Retrieves a customer subscription by ID.", Description = "Fetches a specific customer subscription in the system by its unique ID.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewer)]
    public async Task<IActionResult> GetByIdAsync(Guid id)
    {
        var response = await customerSubscriptionService.GetByIdAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    #endregion

    #region Commands

    /// <summary>
    /// Adds a new customer subscription.
    /// </summary>
    /// <param name="addCustomerSubscriptionRequest">The data for creating the customer subscription.</param>
    /// <returns>The newly added customer subscription.</returns>
    [HttpPost("createCustomerSubscription")]
    [ProducesResponseType(typeof(ApiResponse<CustomerSubscriptionDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [SwaggerOperation(Summary = "Adds a new customer subscription.", Description = "Registers a new customer subscription with the provided details.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdmin)]

    public async Task<IActionResult> AddAsync([FromBody] AddCustomerSubscriptionDto addCustomerSubscriptionRequest)
    {
        if (addCustomerSubscriptionRequest == null)
            return BadRequest(new ApiResponse<string>(StatusCodes.Status400BadRequest, "Request body cannot be null"));

        var response = await customerSubscriptionService.AddAsync(addCustomerSubscriptionRequest);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Updates an existing customer subscription by its ID.
    /// </summary>
    /// <param name="updateCustomerSubscriptionRequest">The data for updating the customer subscription.</param>
    /// <returns>The updated customer subscription.</returns>
    [HttpPut("updateCustomerSubscription")]
    [ProducesResponseType(typeof(ApiResponse<CustomerSubscriptionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Updates an existing customer subscription.", Description = "Updates the details of an existing customer subscription by its ID.")]
    [Authorize(Policy = AuthorizationPolicies.DenyAll)]

    public async Task<IActionResult> UpdateAsync([FromBody] UpdateCustomerSubscriptionDto updateCustomerSubscriptionRequest)
    {
        if (updateCustomerSubscriptionRequest == null)
            return BadRequest(new ApiResponse<string>(StatusCodes.Status400BadRequest, "Invalid request. Customer Subscription ID is required."));

        var response = await customerSubscriptionService.UpdateAsync(updateCustomerSubscriptionRequest);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Deletes a customer subscription by its ID.
    /// </summary>
    /// <param name="id">The ID of the customer subscription to be deleted.</param>
    /// <returns>A 204 No Content response if successful.</returns>
    [HttpDelete("deleteCustomerSubscription/{id:Guid}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Deletes a customer subscription.", Description = "Removes a customer subscription from the system by its ID.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSystemAdmin)]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var response = await customerSubscriptionService.DeleteAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    #endregion
}
