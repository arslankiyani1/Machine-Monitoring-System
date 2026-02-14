namespace MMS.Adapters.PostgreSQL.Repositories;

public class NotificationRepository( ApplicationDbContext dbContext, ILogger<NotificationRepository> logger
) : MMsCrudRepository<Notification>(dbContext, logger), INotificationRepository
{}