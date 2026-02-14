namespace MMS.Application.Common.Traits;

public interface ITrackable : IEntity
{
    public DateTime? CreatedAt { get; }
    public Guid? CreatedBy { get; }
    public DateTime? UpdatedAt { get; }
    public Guid? UpdatedBy { get; }
}