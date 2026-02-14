namespace MMS.Application.Common.Exceptions.Common;

public class AutoMapperResult(IMapper mapper)
{
    public TResult GetResult<TEntity, TResult>(
        Either<RepositoryError, TEntity> response)
    {
        if (response is Right<RepositoryError, TEntity> UserRight)
        {
            TEntity obj = UserRight;
            return mapper.Map<TResult>(obj);
        }
        ThrowErrorIfAny(response);
        return default!;
    }
    public TDestination Map<TDestination>(object source)
    {
        return mapper.Map<TDestination>(source);
    }

    public List<TDestination> Map<TSource, TDestination>(IEnumerable<TSource> source)
    {
        return mapper.Map<List<TDestination>>(source);
    }

    public List<TDestination> MapResult<TSource, TDestination>(List<TSource> source)
    {
        return mapper.Map<List<TDestination>>(source);
    }

    public void GetEmptyResult<TEntity>(Either<RepositoryError, TEntity> response)
    {
        ThrowErrorIfAny(response);
    }

    private void ThrowErrorIfAny<TEntity>(Either<RepositoryError, TEntity> response)
    {
        if (response is Left<RepositoryError, TEntity> left)
        {
            throw left;
        }
    }
    public void Map<TSource, TDestination>(TSource source, TDestination destination)
    {
         mapper.Map(source, destination);
    }
}