namespace MMS.Application.Ports.In.Notification.Dto;

public record UpdateNotificationDto(
    Guid Id,
    NotificationStatus ReadStatus,
    DateTime? ReadAt
);