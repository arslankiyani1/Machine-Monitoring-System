namespace AdapterTwilio;

public class TwilioSettings
{
    public const string SectionName = "Twilio";

    public string AccountSid { get; set; } = string.Empty;
    public string AuthToken { get; set; } = string.Empty;
    public string FromNumber { get; set; } = string.Empty;

    /// <summary>
    /// Message template for offline alerts. Use {MachineName} and {Time} placeholders.
    /// </summary>
    public string OfflineAlertTemplate { get; set; } = "⚠️ Alert: Machine {MachineName} went offline at {Time}. Please check immediately.";

    /// <summary>
    /// Retry settings for transient failures
    /// </summary>
    public RetrySettings Retry { get; set; } = new();

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(AccountSid))
            throw new InvalidOperationException("Twilio AccountSid is required");
        if (string.IsNullOrWhiteSpace(AuthToken))
            throw new InvalidOperationException("Twilio AuthToken is required");
        if (string.IsNullOrWhiteSpace(FromNumber))
            throw new InvalidOperationException("Twilio FromNumber is required");
    }

    public class RetrySettings
    {
        public int MaxRetries { get; set; } = 3;
        public int InitialDelayMs { get; set; } = 1000;
        public int MaxDelayMs { get; set; } = 30000;
    }
}
