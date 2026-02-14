namespace MMS.Application.Common.Exceptions.EntityExceptions;

public class ValidationError(string message) : RepositoryError(message)
{
}