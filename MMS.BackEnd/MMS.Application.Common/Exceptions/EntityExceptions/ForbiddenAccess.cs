namespace MMS.Application.Common.Exceptions.EntityExceptions;

public class ForbiddenAccess(string message) : RepositoryError(message)
{
}