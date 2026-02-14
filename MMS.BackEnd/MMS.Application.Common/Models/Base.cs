namespace MMS.Application.Common.Models;

public abstract class Base : ISoftDelete, IEntity<Guid>
{
    protected DateTime? deleted;
    protected Guid? deletedBy;
    protected Base(Guid id) => Id = id;
    protected Base() { } 
    public Guid Id { get; set; }
    public DateTime? Deleted => deleted;
    public Guid? DeletedBy => deletedBy;
}