namespace MMS.Application.Enum;

public enum TargetAudienceType
{
    All = 0,          // Send to all users
    SpecificUsers = 1, // Send to selected user(s)
    Group = 2,        // Send to a user group/role
    CustomerId = 3      // Send to a specific customer
}
