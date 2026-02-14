namespace MMS.Application.Common.Traits;

public interface ISoftDelete
{
    public DateTime? Deleted { get; }

    public Guid? DeletedBy { get; }
    public bool IsSoftDeleted() => Deleted.HasValue;
}