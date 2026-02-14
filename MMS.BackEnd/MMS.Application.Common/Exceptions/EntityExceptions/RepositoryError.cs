namespace MMS.Application.Common.Exceptions.EntityExceptions;

public class RepositoryError : Exception
{
    public RepositoryError(string message) : base(message)
    {
    }

    public RepositoryError(string message, Exception innerException) : base(message, innerException)
    {
    }
}