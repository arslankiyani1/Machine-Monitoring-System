namespace MMS.Application.Common.Exceptions.EntityExceptions;

public class InvalidFileNameException(string message) : RepositoryError(message)
{
}