namespace MMS.Application.Common.Exceptions.EntityExceptions;

public class InvalidContainerNameException(string message) : RepositoryError(message)
{
}