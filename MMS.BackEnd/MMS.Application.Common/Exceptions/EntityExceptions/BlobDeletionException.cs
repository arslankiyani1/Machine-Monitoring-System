namespace MMS.Application.Common.Exceptions.EntityExceptions;

public class BlobDeletionException(string message, Exception innerException) : RepositoryError(message, innerException)
{
}