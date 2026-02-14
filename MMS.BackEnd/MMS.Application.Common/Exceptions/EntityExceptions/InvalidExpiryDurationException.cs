namespace MMS.Application.Common.Exceptions.EntityExceptions;

public class InvalidExpiryDurationException(string message) : Exception(message)
{
}