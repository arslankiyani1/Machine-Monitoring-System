namespace MMS.Application.Common.Exceptions.EntityExceptions;

public class EntitySoftDeleted : RepositoryError
{
    public EntitySoftDeleted() : base("Entity soft deleted")
    {
    }
}