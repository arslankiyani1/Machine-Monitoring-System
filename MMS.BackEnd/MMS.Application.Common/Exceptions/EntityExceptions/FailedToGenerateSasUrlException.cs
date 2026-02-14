namespace MMS.Application.Common.Exceptions.EntityExceptions;

public class FailedToGenerateSasUrlException(string message, Exception innerException) : RepositoryError(message, innerException)
{
}