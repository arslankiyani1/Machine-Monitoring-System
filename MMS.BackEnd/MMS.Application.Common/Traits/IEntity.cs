namespace MMS.Application.Common.Traits;

public interface IEntity
{
}

public interface IEntity<TKey> : IEntity where TKey : notnull
{
    public TKey Id { get; }
}