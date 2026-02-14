namespace MMS.Application.Common.Extensions;

[ExcludeFromCodeCoverage]
public static class QuerableExtensions
{
    public static IQueryable<T> PaginateList<T>(this IQueryable<T> list, string? term, int? skip, int? top, Expression<Func<T, bool>> expression)
    {
        if (!string.IsNullOrEmpty(term) && term.Length > 2)
        {
            list = list.Where(expression);
        }

        if (skip.HasValue) list = list.Skip(skip.Value);
        if (top.HasValue) list = list.Take(top.Value);

        return list;
    }

    public static IQueryable<TEntity> FilterSoftDelete<TEntity>(this IQueryable<TEntity> query)
        where TEntity : ISoftDelete => query.Where(e => e.Deleted == null);
}