namespace MMS.Application.Ports.Out.Persistence.Interfaces;

public interface ISupportRepository
{
    //Task<List<Support>> GetListAsync(
    //     PageParameters pageParameters,
    //     Expression<Func<Support, bool>> pageFilterExpression,
    //     List<Expression<Func<Support, bool>>>? additionalFilters = null,
    //     Func<IQueryable<Support>, IOrderedQueryable<Support>>? order = null
    // );
    Task<Either<RepositoryError, Support>> GetAsync(Guid supportId);
    Task<Either<RepositoryError, Support>> DeleteAsync(Guid supportId);
    Task<Either<RepositoryError, Support>> AddAsync(Support support);
    Task<Either<RepositoryError, Support>> UpdateAsync(Guid supportId, Support support);

    Task<IEnumerable<Support>> GetListAsync(
       PageParameters pageParameters,
       Expression<Func<Support, bool>> filter = null,
       List<Expression<Func<Support, object>>> includes = null,
       Func<IQueryable<Support>, IOrderedQueryable<Support>> orderBy = null
   );
}
