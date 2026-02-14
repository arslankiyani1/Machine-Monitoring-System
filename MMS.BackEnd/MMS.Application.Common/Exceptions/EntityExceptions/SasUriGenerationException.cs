namespace MMS.Application.Common.Exceptions.EntityExceptions;

public class SasUriGenerationException(string message, Exception innerException) : Exception(message, innerException)
{
}