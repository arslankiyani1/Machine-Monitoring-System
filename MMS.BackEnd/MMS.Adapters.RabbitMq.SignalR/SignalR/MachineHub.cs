namespace MMS.Adapters.RabbitMq.SignalR.SignalR;

public class MachineHub(
    IConnectionMappingService _mappingService,
    IUserService userService,
    ILogger<MachineHub> _logger
) : Hub
{
    public override async Task OnConnectedAsync()
    {
        try
        {
            var httpContext = Context.GetHttpContext();
            if (httpContext == null)
            {
                _logger.LogWarning("HttpContext is null. Cannot process user_id.");
                throw new InvalidOperationException("HttpContext is missing.");
            }

            // ✅ 1. Validate user
            if (!httpContext.Request.Query.TryGetValue("user_id", out var userIdStr) ||
                !Guid.TryParse(userIdStr, out var userId))
            {
                _logger.LogWarning("user_id missing or invalid in query: {Query}", httpContext.Request.Query);
                throw new InvalidOperationException("Invalid or missing user_id.");
            }

            // ✅ 2. Join all accessible customer groups
            var customerIds = await userService.GetAccessibleCustomerIdsAsync(userId);
            if (customerIds.StatusCode != StatusCodes.Status200OK || customerIds.Data == null)
            {
                _logger.LogWarning("No accessible customer IDs found for user: {UserId}", userId);
                throw new InvalidOperationException(customerIds.Message);
            }

            foreach (var customerId in customerIds.Data)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, customerId.ToString());
            }

            // ✅ 3. (NEW) Join specific machine group if provided
            if (httpContext.Request.Query.TryGetValue("machine_name", out var machineName) &&
                !string.IsNullOrWhiteSpace(machineName))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, machineName);
                _logger.LogInformation("User {UserId} joined machine group: {MachineName}", userId, machineName);
            }

            _logger.LogInformation("✅ Connected: {UserId} => {ConnectionId}", userId, Context.ConnectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in OnConnectedAsync");
            throw;
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            _mappingService.Remove(Context.ConnectionId);
            _logger.LogInformation("❌ Disconnected: {ConnectionId}", Context.ConnectionId);

            var httpContext = Context.GetHttpContext();

            // Remove customer groups (get from user_id like in OnConnectedAsync)
            if (httpContext?.Request.Query.TryGetValue("user_id", out var userIdStr) == true &&
                Guid.TryParse(userIdStr, out var userId))
            {
                var customerIds = await userService.GetAccessibleCustomerIdsAsync(userId);
                if (customerIds.StatusCode == StatusCodes.Status200OK && customerIds.Data != null)
                {
                    foreach (var customerId in customerIds.Data)
                    {
                        await Groups.RemoveFromGroupAsync(Context.ConnectionId, customerId.ToString());
                    }
                }
            }

            // Remove machine group if applicable
            if (httpContext?.Request.Query.TryGetValue("machine_name", out var machineName) == true &&
                !string.IsNullOrWhiteSpace(machineName))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, machineName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in OnDisconnectedAsync");
        }

        await base.OnDisconnectedAsync(exception);
    }

    // Broadcast methods
    public async Task SendOrderUpdate(MachineLog log)
    {
        await Clients.All.SendAsync("ReceiveMachineUpdate", log);
    }

    public async Task SendOrderUpdateToClient(string connectionId, MachineLog log)
    {
        await Clients.Client(connectionId).SendAsync("ReceiveMachineUpdate", log);
    }

    public async Task SendOrderUpdateToGroup(Guid customerId, MachineLog log)
    {
        await Clients.Group(customerId.ToString()).SendAsync("ReceiveMachineUpdate", log);
    }

    // ✅ NEW helper for machine name group
    public async Task SendOrderUpdateToMachine(string machineName, MachineLog log)
    {
        await Clients.Group(machineName).SendAsync("ReceiveMachineUpdate", log);
    }
}
