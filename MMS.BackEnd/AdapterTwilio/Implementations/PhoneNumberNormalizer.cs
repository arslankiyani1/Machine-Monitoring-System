using AdapterTwilio.Abstractions;

namespace AdapterTwilio.Implementations;

/// <summary>
/// Service for phone number normalization (Single Responsibility Principle)
/// </summary>
public class PhoneNumberNormalizer : IPhoneNumberNormalizer
{
    public string Normalize(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Phone number cannot be null or empty", nameof(phoneNumber));

        var normalized = phoneNumber.Trim();
        if (!normalized.StartsWith("+"))
            normalized = "+" + normalized;
        return normalized;
    }

    public string Mask(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return "****";

        return phoneNumber.Length > 4 
            ? $"{phoneNumber[..^4]}****" 
            : "****";
    }

    public bool IsValid(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        try
        {
            var normalized = Normalize(phoneNumber);
            return normalized.StartsWith("+") &&
                   normalized.Length >= 10 &&
                   normalized.Length <= 15 &&
                   normalized[1..].All(char.IsDigit);
        }
        catch
        {
            return false;
        }
    }
}
