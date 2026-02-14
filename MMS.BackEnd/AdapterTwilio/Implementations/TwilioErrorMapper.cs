using Twilio.Exceptions;
using AdapterTwilio.Abstractions;

namespace AdapterTwilio.Implementations;

/// <summary>
/// Service for mapping Twilio errors (Single Responsibility Principle)
/// </summary>
public class TwilioErrorMapper : ITwilioErrorMapper
{
    private static readonly int[] TransientErrorCodes = { 20429, 52001, 52002 };

    public bool IsTransientError(ApiException ex)
    {
        if (ex == null) return false;
        return TransientErrorCodes.Contains(ex.Code);
    }

    public string MapError(ApiException ex)
    {
        if (ex == null) return "Unknown error occurred";

        return ex.Code switch
        {
            21211 => "Invalid phone number format",
            21612 => "The recipient is not a valid mobile number",
            21614 => "The recipient cannot receive SMS",
            21408 => "Permission denied for this region",
            21610 => "Message delivery blocked",
            20003 => "Authentication failed",
            20429 => "Rate limit exceeded. Please try again later",
            _ => $"SMS sending failed: {ex.Message}"
        };
    }
}
