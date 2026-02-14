using Microsoft.AspNetCore.Http.HttpResults;

namespace MMS.Application.Ports.In.Notification.Dto;

public record NotificationDto(
    Guid Id,
    string Title,
    string Body,
    List<string> Recipients,
    Guid? MachineId,
    string? MachineName,
    Guid? CustomerId,
    string? Priority,
    string? Link,
    NotificationStatus ReadStatus,
    NotificationTypes NotificationTypes,
    DateTime? ReadAt,
    DateTime? CreatedAt,
    Guid? UserId
)
{
    // Parameterless constructor for serialization
    public NotificationDto() : this(
        Guid.Empty,
        string.Empty,
        string.Empty,
        new List<string>(),
        null,
        null,
        null,
        null,
        null,
        NotificationStatus.Unread,
        NotificationTypes.Alert, // or default enum value
        null,
        null,
        null
    )
    { }
}