using Twilio.Exceptions;

namespace AdapterTwilio.Abstractions;

/// <summary>
/// Interface for mapping Twilio errors (Single Responsibility Principle)
/// </summary>
public interface ITwilioErrorMapper
{
    bool IsTransientError(ApiException ex);
    string MapError(ApiException ex);
}
