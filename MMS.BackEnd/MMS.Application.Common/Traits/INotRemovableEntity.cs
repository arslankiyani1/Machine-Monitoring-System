namespace MMS.Application.Common.Traits;

public interface INotRemovableEntity : IEntity
{
    bool NotRemovable { get; set; }
    public bool IsNotRemovable() => NotRemovable;
}