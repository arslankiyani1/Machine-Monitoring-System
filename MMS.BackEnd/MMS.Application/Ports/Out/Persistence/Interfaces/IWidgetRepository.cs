namespace MMS.Application.Ports.Out.Persistence.Interfaces;

public interface IWidgetRepository
{
    Task<List<Widget>> GetListAsync(
        PageParameters pageParameters,
        Expression<Func<Widget, bool>> pageFilterExpression,
        List<Expression<Func<Widget, bool>>>? documentFilterExpression = null,
        Func<IQueryable<Widget>, IOrderedQueryable<Widget>>? order = null
    );
    Task<Either<RepositoryError, Widget>> GetAsync(Guid widgetId);
    Task<Either<RepositoryError, Widget>> AddAsync(Widget widget);
    Task<Either<RepositoryError, Widget>> UpdateAsync(Guid widgetId, Widget widget);
    Task<Either<RepositoryError, Widget>> DeleteAsync(Guid widgetId);
}
