using Polly.Retry;

namespace AdapterTwilio.Abstractions;

/// <summary>
/// Interface for creating retry policies (Single Responsibility Principle)
/// </summary>
public interface IRetryPolicyFactory
{
    AsyncRetryPolicy CreateRetryPolicy(Action<Exception, TimeSpan, int>? onRetry = null);
}
