namespace MMS.Application.Common.Models;

public abstract class Trackable : Base, ITrackable, IEntity
{
    protected Trackable(Guid id) : base(id) { }
    protected Trackable() { }

    protected DateTime? createdAt;
    protected Guid? createdBy;
    protected DateTime? updatedAt;
    protected Guid? updatedBy;

    public DateTime? CreatedAt => createdAt;
    public Guid? CreatedBy => createdBy;
    public DateTime? UpdatedAt => updatedAt;
    public Guid? UpdatedBy => updatedBy;
}