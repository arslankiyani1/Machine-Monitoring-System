namespace MMS.Application.Common.Exceptions.EntityExceptions;

public class BlobContainerCreationException(string message, Exception innerException) : RepositoryError(message, innerException)
{
}