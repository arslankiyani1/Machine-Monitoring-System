namespace MMS.Application.Common.Exceptions.EntityExceptions;

public class BlobNotFoundException(string message) : RepositoryError(message)
{
}