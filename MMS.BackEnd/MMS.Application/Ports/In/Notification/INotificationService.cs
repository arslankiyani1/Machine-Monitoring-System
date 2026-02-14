
namespace MMS.Application.Ports.In.Notification;

public interface INotificationService
{
    Task<ApiResponse<IEnumerable<NotificationDto>>> GetListAsync(PageParameters pageParameters, NotificationStatus? status);
    Task<ApiResponse<NotificationDto>> GetByIdAsync(Guid notificationId);
    Task<ApiResponse<NotificationDto>> AddAsync(AddNotificationDto request,bool? support);
    Task<ApiResponse<NotificationDto>> AddAlertAsync(AddNotificationDto request, CancellationToken cancellationToken);

    Task<ApiResponse<NotificationDto>> UpdateAsync(Guid notificationId, UpdateNotificationDto request);
    Task<ApiResponse<string>> DeleteAsync(Guid notificationId);
    Task<ApiResponse<NotificationDto>> MarkAsReadAsync(MarkNotificationReadDto dto);
}