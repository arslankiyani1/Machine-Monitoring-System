namespace MMS.Application.Ports.In.Notification.Dto;

public record AddNotificationDto(
    string Title,
    string Body,
    List<string> Recipients,
    Guid? MachineId,
    string? MachineName,
    Guid? CustomerId,
    string? Priority,
    string? Link,
    NotificationTypes NotificationTypes,
    List<Guid?> UserIds
);