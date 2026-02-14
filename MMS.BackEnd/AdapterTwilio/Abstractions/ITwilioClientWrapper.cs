using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace AdapterTwilio.Abstractions;

/// <summary>
/// Abstraction for Twilio client operations (Dependency Inversion Principle)
/// </summary>
public interface ITwilioClientWrapper
{
    Task<MessageResource> SendMessageAsync(string body, PhoneNumber from, PhoneNumber to);
    void Initialize(string accountSid, string authToken);
}
