namespace MMS.Application.Common.Exceptions.EntityExceptions;

public class InvalidContentException(string message) : RepositoryError(message)
{
}