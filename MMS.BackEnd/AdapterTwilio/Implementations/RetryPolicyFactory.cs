using Polly;
using Polly.Retry;
using Twilio.Exceptions;
using AdapterTwilio.Abstractions;
using Microsoft.Extensions.Logging;

namespace AdapterTwilio.Implementations;

/// <summary>
/// Factory for creating retry policies (Single Responsibility Principle)
/// </summary>
public class RetryPolicyFactory : IRetryPolicyFactory
{
    private readonly ITwilioErrorMapper _errorMapper;
    private readonly TwilioSettings _settings;
    private readonly ILogger<RetryPolicyFactory> _logger;

    public RetryPolicyFactory(
        ITwilioErrorMapper errorMapper,
        Microsoft.Extensions.Options.IOptions<TwilioSettings> settings,
        ILogger<RetryPolicyFactory> logger)
    {
        _errorMapper = errorMapper ?? throw new ArgumentNullException(nameof(errorMapper));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public AsyncRetryPolicy CreateRetryPolicy(Action<Exception, TimeSpan, int>? onRetry = null)
    {
        return Policy
            .Handle<ApiException>(ex => _errorMapper.IsTransientError(ex))
            .WaitAndRetryAsync(
                _settings.Retry.MaxRetries,
                retryAttempt => TimeSpan.FromMilliseconds(
                    Math.Min(
                        _settings.Retry.InitialDelayMs * Math.Pow(2, retryAttempt - 1),
                        _settings.Retry.MaxDelayMs)),
                onRetry: (exception, timeSpan, retryCount, _) =>
                {
                    _logger.LogWarning(
                        exception,
                        "Twilio API retry {RetryCount} after {Delay}ms",
                        retryCount, timeSpan.TotalMilliseconds);
                    
                    onRetry?.Invoke(exception, timeSpan, retryCount);
                });
    }
}
