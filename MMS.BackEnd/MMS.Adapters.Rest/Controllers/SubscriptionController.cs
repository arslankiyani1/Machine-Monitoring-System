using MMS.Application.Ports.In.Payment_Method;

namespace MMS.Adapters.Rest.Controllers;

[Route("api/[controller]")]
[ApiController]
//[Authorize(Roles = ApplicationRoles.User)]
public class SubscriptionController(ISubscriptionService subscriptionService) : BaseController
{
    #region Queries

    /// <summary>
    /// Retrieves a list of all subscriptions.
    /// </summary>
    [HttpGet("getAllSubscriptions")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<SubscriptionDto>>), StatusCodes.Status200OK)]
    [SwaggerOperation(Summary = "Retrieves a list of all subscriptions.", Description = "Fetches all subscriptions available in the system.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewer)]

    public async Task<IActionResult> GetListAsync([FromQuery] PageParameters pageParameters,
        [FromQuery] SubscriptionStatus? status)
    {
        var response = await subscriptionService.GetListAsync(pageParameters, status);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Retrieves a subscription by ID.
    /// </summary>
    [HttpGet("getById/{id:Guid}")]
    [ProducesResponseType(typeof(ApiResponse<SubscriptionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Retrieves a subscription by ID.", Description = "Fetches a specific subscription by its unique ID.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewer)]
    public async Task<IActionResult> GetByIdAsync(Guid id)
    {
        var response = await subscriptionService.GetByIdAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    #endregion

    #region Commands

    /// <summary>
    /// Processes the payment for a subscription.
    /// </summary>
    /// <param name="dto">The subscription payment details.</param>
    /// <returns>The result of the subscription payment processing.</returns>
    [HttpPost("process-subscription")]
    [ProducesResponseType(typeof(ApiResponse<SubscriptionPaymentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(
        Summary = "Processes a subscription payment.",
        Description = "Handles the payment for an existing subscription and updates its status accordingly."
    )]
    [Authorize(Policy = AuthorizationPolicies.RequireSystemAdmin)]
    public async Task<IActionResult> ProcessSubscriptionPayment([FromBody] SubscriptionPaymentDto dto)
    {
        var response = await subscriptionService.ProcessSubscriptionPaymentAsync(dto);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Adds a new subscription.
    /// </summary>
    [HttpPost("createSubscription")]
    [ProducesResponseType(typeof(ApiResponse<SubscriptionDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [SwaggerOperation(Summary = "Adds a new subscription.", Description = "Registers a new subscription with the provided details.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSystemAdmin)]

    public async Task<IActionResult> AddAsync([FromBody] SubscriptionAddDto addSubscriptionRequest)
    {
        if (addSubscriptionRequest == null)
            return BadRequest(new ApiResponse<string>(StatusCodes.Status400BadRequest, "Request body cannot be null"));

        var response = await subscriptionService.AddAsync(addSubscriptionRequest);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Updates an existing subscription.
    /// </summary>
    [HttpPut("updateSubscription")]
    [ProducesResponseType(typeof(ApiResponse<SubscriptionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Updates an existing subscription.", Description = "Updates the details of an existing subscription by its ID.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSystemAdmin)]

    public async Task<IActionResult> UpdateAsync([FromBody] SubscriptionUpdateDto updateSubscriptionRequest)
    {
        if (updateSubscriptionRequest == null)
            return BadRequest(new ApiResponse<string>(StatusCodes.Status400BadRequest, "Invalid request. Subscription ID is required."));

        var response = await subscriptionService.UpdateAsync(updateSubscriptionRequest.Id, updateSubscriptionRequest);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Deletes a subscription by its ID.
    /// </summary>
    [HttpDelete("deleteSubscription/{id:Guid}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Deletes a subscription.", Description = "Removes a subscription from the system by its ID.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSystemAdmin)]

    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var response = await subscriptionService.DeleteAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    #endregion
}