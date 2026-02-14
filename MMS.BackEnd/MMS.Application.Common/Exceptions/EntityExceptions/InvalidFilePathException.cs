namespace MMS.Application.Common.Exceptions.EntityExceptions;

public class InvalidFilePathException : RepositoryError
{
    public InvalidFilePathException(string message) : base(message) { }

    public InvalidFilePathException(string message, Exception innerException)
        : base(message, innerException) { }
}