namespace MMS.Adapters.PostgreSQL.Repositories;

public class SupportRepository(
        ApplicationDbContext dbContext,
        ILogger<SupportRepository> logger)
        : MMsCrudRepository<Support>(dbContext, logger), ISupportRepository
{
    public async Task<IEnumerable<Support>> GetListAsync(
         PageParameters pageParameters,
         Expression<Func<Support, bool>>? filter,
         List<Expression<Func<Support, object>>>? includes,
         Func<IQueryable<Support>, IOrderedQueryable<Support>>? orderBy)
    {
        IQueryable<Support> query = dbContext.Supports.AsNoTracking();

        if (filter != null)
        {
            query = query.Where(filter);
        }

        if (includes != null && includes.Count > 0)
        {
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
        }

        if (orderBy != null)
        {
            query = orderBy(query);
        }

        var skip = pageParameters?.Skip ?? 0;
        var take = pageParameters?.Top ?? 10;
        query = query.Skip(skip).Take(take);

        return await query.ToListAsync();
    }
}