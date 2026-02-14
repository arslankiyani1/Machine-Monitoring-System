using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using AdapterTwilio.Abstractions;
using Microsoft.Extensions.Logging;

namespace AdapterTwilio.Implementations;

/// <summary>
/// Wrapper for Twilio client (Dependency Inversion Principle)
/// </summary>
public class TwilioClientWrapper : ITwilioClientWrapper
{
    private static bool _initialized;
    private static readonly object _lock = new();
    private readonly ILogger<TwilioClientWrapper> _logger;

    public TwilioClientWrapper(ILogger<TwilioClientWrapper> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void Initialize(string accountSid, string authToken)
    {
        if (_initialized) return;

        lock (_lock)
        {
            if (_initialized) return;
            TwilioClient.Init(accountSid, authToken);
            _initialized = true;
            _logger.LogInformation("Twilio client initialized");
        }
    }

    public async Task<MessageResource> SendMessageAsync(string body, PhoneNumber from, PhoneNumber to)
    {
        return await MessageResource.CreateAsync(
            body: body,
            from: from,
            to: to
        );
    }
}
