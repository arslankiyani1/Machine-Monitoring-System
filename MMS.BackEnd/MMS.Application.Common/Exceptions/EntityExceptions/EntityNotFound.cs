namespace MMS.Application.Common.Exceptions.EntityExceptions;

public class EntityNotFound : RepositoryError
{
    public EntityNotFound() : base("Entity not found error occurred while accessing the repository.")
    {
    }

    public EntityNotFound(string message) : base(message)
    {
    }
}