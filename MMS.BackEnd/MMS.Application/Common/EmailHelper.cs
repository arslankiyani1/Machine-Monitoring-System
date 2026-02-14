namespace MMS.Application.Common;

[ExcludeFromCodeCoverage]
public static class EmailHelper
{
    public static string AnonymizeEmail(string? email)
    {
        if (string.IsNullOrEmpty(email))
            return $"anonymized+{Guid.NewGuid()}@example.com";
        var parts = email.Split('@');
        if (parts.Length == 2)
        {
            return $"anonymized+{Guid.NewGuid()}@{parts[1]}";
        }
        return email;
    }

    public static string GetEmailDomain(string? email)
    {
        if (string.IsNullOrEmpty(email))
            return string.Empty;
        return email.Split('@').LastOrDefault()?.ToLower() ?? string.Empty;
    }
}