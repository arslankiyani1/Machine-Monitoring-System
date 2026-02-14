namespace MMS.Application.Common.Exceptions.EntityExceptions;

public class EntityConflict : RepositoryError
{
    public EntityConflict() : base("Conflict occurred while accessing the entity.")
    {
    }

    public EntityConflict(string message) : base(message)
    {
    }
}