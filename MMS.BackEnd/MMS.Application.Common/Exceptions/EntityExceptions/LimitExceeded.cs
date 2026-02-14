namespace MMS.Application.Common.Exceptions.EntityExceptions;

public class LimitExceeded(string message) : RepositoryError(message)
{
}