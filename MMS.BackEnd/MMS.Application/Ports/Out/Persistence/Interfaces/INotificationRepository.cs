namespace MMS.Application.Ports.Out.Persistence.Interfaces;

public interface INotificationRepository
{
    Task<List<Notification>> GetListAsync(
        PageParameters pageParameters,
        Expression<Func<Notification, bool>> pageFilterExpression,
        List<Expression<Func<Notification, bool>>>? documentFilterExpression = null,
        Func<IQueryable<Notification>, IOrderedQueryable<Notification>>? order = null
    );
    Task<Either<RepositoryError, Notification>> GetAsync(Guid notificationId);
    Task<Either<RepositoryError, Notification>> DeleteAsync(Guid notificationId);
    Task<Either<RepositoryError, Notification>> AddAsync(Notification notification);
    Task<Either<RepositoryError, Notification>> UpdateAsync(Guid notificationId, Notification notification);
}
