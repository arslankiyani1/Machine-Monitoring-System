namespace MMS.Application.Utils;

public static class CustomerAccessHelper
{
    public static List<Guid>? GetAccessibleCustomerIds(IUserContextService userContextService)
    {
        if (string.Equals(userContextService.Role, ApplicationRoles.RoleSystemAdmin, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return userContextService.CustomerIds?
            .Select(Guid.Parse)
            .ToList();
    }

    public static async Task ValidateCustomerAccessAsync(IUserContextService userContextService, Guid customerId)
    {
        if (!userContextService.Role.Equals(ApplicationRoles.RoleSystemAdmin, StringComparison.OrdinalIgnoreCase) &&
            !userContextService.Role.Equals(ApplicationRoles.RoleMMSBridge, StringComparison.OrdinalIgnoreCase) &&
            !userContextService.CustomerIds.Select(id => Guid.Parse(id)).Contains(customerId))
        {
            throw new UnauthorizedAccessException("Access denied to this customer.");
        }
        await Task.CompletedTask;
    }

    public static async Task ValidateCustomerAccessAsync(IUserContextService userContextService, List<string>? customerIds)
    {
        // If no customerIds are provided, nothing to validate
        if (customerIds == null || !customerIds.Any())
        {
            await Task.CompletedTask;
            return;
        }

        if (string.Equals(userContextService.Role, ApplicationRoles.RoleSystemAdmin, StringComparison.OrdinalIgnoreCase))
        {
            await Task.CompletedTask;
            return; // System Admin has access to all customers
        }
        var userCustomerIds = userContextService.CustomerIds?.ToHashSet() ?? new HashSet<string>();
        var unauthorizedIds = customerIds
            .Where(id => !userCustomerIds.Contains(id))
            .ToList();
        if (unauthorizedIds.Any())
        {
            throw new UnauthorizedAccessException($"Access denied to customers: {string.Join(", ", unauthorizedIds)}");
        }

        await Task.CompletedTask;
    }



    public static List<Guid>? GetAccessibleCustomerAllowMMSBridgeIds(IUserContextService userContextService)
    {
        if (string.Equals(userContextService.Role, ApplicationRoles.RoleSystemAdmin, StringComparison.OrdinalIgnoreCase)
            ||
           userContextService.Role.Equals(ApplicationRoles.RoleMMSBridge, StringComparison.OrdinalIgnoreCase)
            )
        {
            return null; // System admin and MMSBridge has access to all customers for sensor and machine logs
        }

        return userContextService.CustomerIds?
            .Select(Guid.Parse)
            .ToList();
    }
    public static async Task ValidateCustomerAccessAllowMMSBridgeAsync(IUserContextService userContextService, Guid customerId)
    {
        if (!userContextService.Role.Equals(ApplicationRoles.RoleSystemAdmin, StringComparison.OrdinalIgnoreCase) &&
            !userContextService.Role.Equals(ApplicationRoles.RoleMMSBridge, StringComparison.OrdinalIgnoreCase) &&
            !userContextService.CustomerIds.Select(id => Guid.Parse(id)).Contains(customerId))
        {
            throw new UnauthorizedAccessException("Access denied to this customer.");
        }
        await Task.CompletedTask;
    }
}