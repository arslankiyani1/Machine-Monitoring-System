namespace MMS.Application.Common.Exceptions.EntityExceptions;

public class BlobUploadException(string message, Exception innerException) : RepositoryError(message, innerException)
{
}