namespace MMS.Application.Common.Repositories;

[ExcludeFromCodeCoverage]
public class MMsCrudRepository<TEntity>(DbContext dbContext, ILogger logger,
    Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null) : MMSBaseRepository<TEntity, Guid>(dbContext, logger, include) where TEntity : class, IEntity<Guid>;