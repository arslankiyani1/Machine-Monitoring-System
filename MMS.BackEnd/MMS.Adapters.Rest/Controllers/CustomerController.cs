namespace MMS.Adapters.Rest.Controllers;

[Route("api/[controller]")]
[ApiController]
//[Authorize(Roles = ApplicationRoles.User)]
public class CustomerController(ICustomerService customerService) : BaseController
{
    #region Queries

    /// <summary>
    /// Retrieves customer dashboard cards.
    /// </summary>
    /// <remarks>
    /// This endpoint returns dashboard card information for customers.
    /// Useful for visual summary or dashboard display.
    /// </remarks>
    /// <returns>List of customer cards with summarized data.</returns>
    [HttpGet("customers/cards")]
    [ProducesResponseType(typeof(ApiResponse<List<CustomerCardDto>>), StatusCodes.Status200OK)]
    [SwaggerOperation(Summary = "Gets customer dashboard cards", Description = "Retrieves summarized customer data for dashboard display.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewerOrTechnicianOrOperator)]
    public async Task<IActionResult> GetCustomerCards([FromQuery] PageParameters pageParameters, [FromQuery] Guid? customerId)
    {
        var response = await customerService.GetCustomerDashboardAsync(pageParameters, customerId);
        return StatusCode(response.StatusCode, response);
    }



    /// <summary>
    /// Retrieves customer dashboard cards complete details.
    /// </summary>
    /// <remarks>
    /// This endpoint returns dashboard card complete information for customers.
    /// Useful for visual summary or dashboard display.
    /// </remarks>
    /// <returns>List of customer cards with summarized data.</returns>
    [HttpGet("customers/cards-details")]
    [ProducesResponseType(typeof(ApiResponse<List<CustomerCardDto>>), StatusCodes.Status200OK)]
    [SwaggerOperation(Summary = "Gets customer dashboard cards", Description = "Retrieves summarized customer data for dashboard display.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewerOrTechnicianOrOperator)]
    public async Task<IActionResult> GetCustomerCardsDetails([FromQuery] PageParameters pageParameters)
    {
        var response = await customerService.GetCustomerDashboardDetailsAsync(pageParameters);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Retrieves a summary of machine statuses for a specific customer.
    /// </summary>
    /// <remarks>
    /// This endpoint returns the latest machine status summary for a given customer.
    /// Useful for showing customer-level dashboard KPIs.
    /// </remarks>
    /// <param name="customerId">The ID of the customer.</param>
    /// <returns>A dictionary of machine status counts.</returns>
    [HttpGet("customer-summary/{customerId}")]
    [ProducesResponseType(typeof(ApiResponse<Dictionary<string, int>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<Dictionary<string, int>>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<Dictionary<string, int>>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<Dictionary<string, int>>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Gets customer card summary", Description = "Retrieves machine status summary for a given customer.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewerOrTechnicianOrOperator)]
    public async Task<IActionResult> GetCustomerCardSummary(Guid customerId)
    {
        var response = await customerService.GetCustomerSummaryAsync(customerId);
        return StatusCode(response.StatusCode, response);
    }



    /// <summary>
    /// Retrieves all machines for a specific customer.
    /// </summary>
    /// <remarks>
    /// Provide a valid customer ID to fetch all related machines.
    /// Returns 404 if no machines are found.
    /// </remarks>
    /// <param name="customerId">Unique identifier of the customer.</param>
    /// <returns>List of machine details.</returns>
    [HttpGet("getMachinesByCustomerId/{customerId:Guid}")]
    [ProducesResponseType(typeof(ApiResponse<List<MachineDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(
        Summary = "Gets all machines by customer ID",
        Description = "Retrieves a list of machines associated with the specified customer."
    )]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewerOrTechnicianOrOperator)]
    public async Task<IActionResult> GetMachinesByCustomerId([FromQuery] PageParameters pageParameters, 
        Guid customerId, DateTime from, DateTime to)
    {
        var response = await customerService.GetMachinesByCustomerIdAsync(customerId, pageParameters,from,to);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Retrieves a list of all customers.
    /// </summary>
    /// <remarks>
    /// This endpoint returns a list of all customers stored in the system.
    /// </remarks>
    /// <returns>A list of customers.</returns>
    [HttpGet("getAllCustomers")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CustomerDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Retrieves all customers", Description = "Fetches the complete list of registered customers.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewerOrTechnicianOrOperator)]
    public async Task<IActionResult> GetListAsync([FromQuery] PageParameters pageParameters, [FromQuery] CustomerStatus? customerStatus)
    {
        var response = await customerService.GetListAsync(pageParameters, customerStatus);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Retrieves a specific customer by ID.
    /// </summary>
    /// <remarks>
    /// Provide a valid customer ID to fetch details.  
    /// If the customer does not exist, a 404 error is returned.
    /// </remarks>
    /// <param name="id">Unique identifier of the customer.</param>
    /// <returns>Customer details.</returns>
    [HttpGet("getById/{id:Guid}")]
    [ProducesResponseType(typeof(ApiResponse<CustomerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Retrieves a customer by ID", Description = "Fetches details of a specific customer using their unique identifier.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewerOrTechnicianOrOperator)]
    public async Task<IActionResult> GetByIdAsync(Guid id)
    {
        var response = await customerService.GetByIdAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    #endregion

    #region Commands

    /// <summary>
    /// Adds a new customer to the system.
    /// </summary>
    /// <remarks>
    /// This endpoint registers a new customer using the provided details.
    /// </remarks>
    /// <param name="addCustomerRequest">The details of the new customer.</param>
    /// <returns>The newly created customer.</returns>
    [HttpPost("createCustomer")]
    [ProducesResponseType(typeof(ApiResponse<CustomerDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [SwaggerOperation(Summary = "Creates a new customer", Description = "Registers a new customer and returns their details.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSystemAdmin)]
    public async Task<IActionResult> AddAsync([FromBody] AddCustomerDto addCustomerRequest)
    {
        if (addCustomerRequest == null)
        {
            return BadRequest(new ApiResponse<string>(StatusCodes.Status400BadRequest, "Request body cannot be null"));
        }

        var response = await customerService.AddAsync(addCustomerRequest);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Updates an existing customer.
    /// </summary>
    /// <remarks>
    /// Provide a valid customer ID and update details to modify an existing customer record.
    /// </remarks>
    /// <param name="id">The unique identifier of the customer.</param>
    /// <param name="updateCustomerRequest">The new data for the customer.</param>
    /// <returns>The updated customer details.</returns>
    [HttpPut("updateCustomer")]
    [ProducesResponseType(typeof(ApiResponse<CustomerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [SwaggerOperation(Summary = "Updates an existing customer", Description = "Modifies the details of an existing customer using their unique ID.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSysAdminOrCustAdmin)]
    public async Task<IActionResult> UpdateAsync([FromBody] UpdateCustomerDto updateCustomerRequest)
    {
        var response = await customerService.UpdateAsync(updateCustomerRequest.Id, updateCustomerRequest);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Deletes a customer by ID.
    /// </summary>
    /// <remarks>
    /// This endpoint removes a customer from the system permanently.
    /// </remarks>
    /// <param name="id">The unique identifier of the customer.</param>
    /// <returns>A success message if the deletion is successful.</returns>
    [HttpDelete("deleteCustomer/{id:Guid}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Deletes a customer", Description = "Removes a customer from the system based on their unique ID.")]
    [Authorize(Policy = AuthorizationPolicies.RequireSystemAdmin)]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var response = await customerService.DeleteAsync(id);
        return StatusCode(response.StatusCode, response);
    }
    #endregion
}

