namespace MMS.Application.Common.Repositories;

[ExcludeFromCodeCoverage]
public abstract class MMSBaseRepository<TEntity, TKey>(DbContext dbContext, ILogger logger,
    Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null) where TEntity : class, IEntity<TKey> where TKey : notnull
{
    #region Ctor
    private readonly DbSet<TEntity> _dbSet = dbContext.Set<TEntity>();
    public Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? Include { get; set; } = include;
    #endregion

    #region GetAllPaginated
    public virtual async Task<List<TEntity>> GetListAsync(PageParameters? pageParameters,
        Expression<Func<TEntity, bool>> pageParametersExpression,
        List<Expression<Func<TEntity, bool>>>? documentFilterExpression,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? order = null)
    {
        var query = _dbSet.AsNoTracking();

        if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)))
        {
            query = query.Cast<ISoftDelete>().FilterSoftDelete().Cast<TEntity>();
        }
        
        if (documentFilterExpression != null)
            query = documentFilterExpression.Aggregate(query, (current, expression) => current.Where(expression));
        if (Include != null) query = Include(query);
        if (order != null) query = order(query);
        if (pageParameters != null)
            query = query.PaginateList(pageParameters.Term, pageParameters.Skip, pageParameters.Top,
               pageParametersExpression);
        return await query.ToListAsync();
    }
    #endregion

    #region GetAll
    public virtual async Task<IEnumerable<TEntity>> GetListAsync()
    {
        var query = _dbSet.AsNoTracking();
        if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)))
        {
            query = query.Cast<ISoftDelete>().FilterSoftDelete().Cast<TEntity>();
        }
        return await query.ToListAsync();
    }
    #endregion

    #region GetById
    public virtual async Task<Either<RepositoryError, TEntity>> GetAsync(TKey id)
    {
        var query = _dbSet.AsTracking(QueryTrackingBehavior.TrackAll);
        if (Include != null) query = Include(query);
        var entity = await query.FirstOrDefaultAsync(t => t.Id.Equals(id));
        return entity switch
        {
            null => new EntityNotFound(),
            ISoftDelete softDeleteEntity when softDeleteEntity.IsSoftDeleted() => new EntitySoftDeleted(),
            _ => entity
        };
    }

    #endregion: 

    #region Create
    public virtual async Task<Either<RepositoryError, TEntity>> AddAsync(TEntity entity)
    {
        _dbSet.Add(entity);
        try
        {
            await dbContext.SaveChangesAsync();
            logger.LogInformation("Entity of type {EntityType} with ID {EntityId} inserted successfully.", typeof(TEntity).Name, entity.Id);
            return entity;
        }
        catch (DbUpdateException e) when (e.InnerException is PostgresException)
        {
            await dbContext.Entry(entity).ReloadAsync();
            return entity switch
            {
                ISoftDelete softDeleteEntity when softDeleteEntity.IsSoftDeleted() => new EntitySoftDeleted(),
                _ => new EntityConflict()
            };
        }
    }
    #endregion

    #region Update
    public virtual async Task<Either<RepositoryError, TEntity>> UpdateAsync(TKey id, TEntity entity)
    {
        var existingEntity = await _dbSet.FindAsync(id);
        switch (existingEntity)
        {
            case null:
                return new EntityNotFound();
            case ISoftDelete softDeleteEntity when softDeleteEntity.IsSoftDeleted():
                return new EntitySoftDeleted();
        }
        dbContext.Attach(entity);
        dbContext.Entry(entity).State = EntityState.Modified;
        dbContext.Entry(entity).Property(nameof(ITrackable.CreatedAt)).IsModified = false;
        dbContext.Entry(entity).Property(nameof(ITrackable.CreatedBy)).IsModified = false;
        try
        {
            await dbContext.SaveChangesAsync();
            return entity;
        }
        catch (DbUpdateException e) when (e.InnerException is PostgresException sqlException)
        {
            var conflictEntity = await _dbSet.AsQueryable().FirstOrDefaultAsync(t => t.Id.Equals(entity.Id));
            return conflictEntity switch
            {
                ISoftDelete softDeleteEntity when softDeleteEntity.IsSoftDeleted() => new EntitySoftDeleted(),
                _ => new EntityConflict()
            };
        }
    }
    #endregion

    #region Delete
    public virtual async Task<Either<RepositoryError, TEntity>> DeleteAsync(TKey id)
    {
        var entityToDelete = await _dbSet.FindAsync(id);
        switch (entityToDelete)
        {
            case null:
                return new EntityNotFound();
            case INotRemovableEntity notRemovableEntity when notRemovableEntity.IsNotRemovable():
                return new EntityNotRemovable();
            case ISoftDelete softDeleteEntity when softDeleteEntity.IsSoftDeleted():
                return new EntitySoftDeleted();
            case ISoftDelete:
                dbContext.Entry(entityToDelete).State = EntityState.Deleted;
                dbContext.Entry(entityToDelete).Property(nameof(ITrackable.CreatedAt)).IsModified = false;
                dbContext.Entry(entityToDelete).Property(nameof(ITrackable.CreatedBy)).IsModified = false;
                break;
            default:
                dbContext.Entry(entityToDelete).State = EntityState.Deleted;
                break;
        }
        await dbContext.SaveChangesAsync();
        return entityToDelete;
    }


    #endregion

    #region Count
    public virtual async Task<int> GetTotalCountAsync(List<Expression<Func<TEntity, bool>>> filterExpressions = null!)
    {
        var query = _dbSet.AsNoTracking();
        if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)))
        {
            query = query.Cast<ISoftDelete>().FilterSoftDelete().Cast<TEntity>();
        }
        if (filterExpressions != null)
            query = filterExpressions.Aggregate(query, (current, expression) => current.Where(expression));
        return await query.CountAsync();
    }
    #endregion

    public virtual async Task<bool> ExistsIgnoreCaseAsync(Expression<Func<TEntity, string>> propertySelector, string value)
    {
        var parameter = Expression.Parameter(typeof(TEntity), "x");
        var left = Expression.Call(Expression.Invoke(propertySelector, parameter), nameof(string.ToLower), null);
        var right = Expression.Constant(value.ToLower());

        var equals = Expression.Equal(left, right);
        var lambda = Expression.Lambda<Func<TEntity, bool>>(equals, parameter);

        var query = _dbSet.AsNoTracking();
        if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)))
        {
            query = query.Cast<ISoftDelete>().FilterSoftDelete().Cast<TEntity>();
        }

        return await query.AnyAsync(lambda);
    }
}