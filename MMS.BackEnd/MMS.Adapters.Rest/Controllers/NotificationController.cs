namespace MMS.Adapters.Rest.Controllers;

[Route("api/[controller]")]
[ApiController]
//[Authorize(Roles = ApplicationRoles.User)]
public class NotificationController(INotificationService notificationService) : BaseController
{
    #region Queries

    /// <summary>
    /// Retrieves a list of all notifications.
    /// </summary>
    /// <returns>A list of all notifications.</returns>
    [HttpGet("getAllNotifications")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<NotificationDto>>), StatusCodes.Status200OK)]
    [SwaggerOperation(Summary = "Retrieves a list of all notifications.", Description = "Fetches all notifications available in the system.")]
    public async Task<IActionResult> GetListAsync([FromQuery] PageParameters pageParameters,
        [FromQuery] NotificationStatus? status)
    {
        var response = await notificationService.GetListAsync(pageParameters,status);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Retrieves a notification by ID.
    /// </summary>
    [HttpGet("getById/{id:Guid}")]
    [ProducesResponseType(typeof(ApiResponse<NotificationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Retrieves a notification by ID.", Description = "Fetches a specific notification in the system by its unique ID.")]
    public async Task<IActionResult> GetByIdAsync(Guid id)
    {
        var response = await notificationService.GetByIdAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    #endregion

    #region Commands

    /// <summary>
    /// Marks a notification as read.
    /// </summary>
    /// <param name="dto">The data transfer object containing the notification ID to mark as read.</param>
    /// <returns>A 200 OK response with the updated notification if successful; otherwise, a 404 or 500 response.</returns>
    [HttpPut("mark-as-read")]
    [ProducesResponseType(typeof(ApiResponse<NotificationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Marks a notification as read.",
        Description = "Updates the specified notification to mark it as read by setting its ReadAt timestamp. If the notification is already read, returns the current state."
    )]
    public async Task<IActionResult> MarkAsRead([FromBody] MarkNotificationReadDto dto)
    {
        var response = await notificationService.MarkAsReadAsync(dto);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Adds a new notification.
    /// </summary>
    /// <param name="addNotificationRequest">The data for creating the notification.</param>
    /// <returns>The newly added notification.</returns>
    [HttpPost("createNotification")]
    [ProducesResponseType(typeof(ApiResponse<NotificationDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [SwaggerOperation(Summary = "Adds a new notification.", Description = "Registers a new notification with the provided details.")]
    public async Task<IActionResult> AddAsync([FromBody] AddNotificationDto addNotificationRequest)
    {
        if (addNotificationRequest == null)
            return BadRequest(new ApiResponse<string>(StatusCodes.Status400BadRequest, "Request body cannot be null"));

        var response = await notificationService.AddAsync(addNotificationRequest, true);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Updates an existing notification by its ID.
    /// </summary>
    /// <param name="updateNotificationRequest">The data for updating the notification.</param>
    /// <returns>The updated notification.</returns>
    [HttpPut("updateNotification")]
    [ProducesResponseType(typeof(ApiResponse<NotificationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Updates an existing notification.", Description = "Updates the details of an existing notification by its ID.")]
    public async Task<IActionResult> UpdateAsync([FromBody] UpdateNotificationDto updateNotificationRequest)
    {
        if (updateNotificationRequest == null)
            return BadRequest(new ApiResponse<string>(StatusCodes.Status400BadRequest, "Invalid request. Notification ID is required."));

        var response = await notificationService.UpdateAsync(updateNotificationRequest.Id, updateNotificationRequest);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Deletes a notification by its ID.
    /// </summary>
    /// <param name="id">The ID of the notification to be deleted.</param>
    /// <returns>A 204 No Content response if successful.</returns>
    [HttpDelete("deleteNotification/{id:Guid}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Deletes a notification.", Description = "Removes a notification from the system by its ID.")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var response = await notificationService.DeleteAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    #endregion
}