namespace MMS.Application.Interfaces;

public interface IRealtimeNotificationService
{
    Task NotifyUserAsync(NotificationDto notification);
}