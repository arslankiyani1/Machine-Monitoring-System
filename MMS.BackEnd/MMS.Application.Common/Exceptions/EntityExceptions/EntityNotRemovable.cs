namespace MMS.Application.Common.Exceptions.EntityExceptions;

public class EntityNotRemovable : RepositoryError
{
    public EntityNotRemovable() : base("Entity not removable error occurred while accessing the repository.")
    {
    }

    public EntityNotRemovable(string message) : base(message)
    {
    }
}