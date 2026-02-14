namespace AdapterTwilio.Abstractions;

/// <summary>
/// Interface for phone number normalization (Single Responsibility Principle)
/// </summary>
public interface IPhoneNumberNormalizer
{
    string Normalize(string phoneNumber);
    string Mask(string phoneNumber);
    bool IsValid(string phoneNumber);
}
