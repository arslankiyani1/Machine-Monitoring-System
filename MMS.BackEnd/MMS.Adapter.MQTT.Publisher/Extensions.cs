namespace MMS.Adapter.MQTT.Publisher;

public static class Extensions
{
    public static string GenerateSasToken(string iotHubHostName, string deviceId, string deviceKey, int ttlSeconds)
    {
        string resource = $"{iotHubHostName.ToLowerInvariant()}/devices/{deviceId}";
        string encodedResource = Uri.EscapeDataString(resource);

        long expiry = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + ttlSeconds;
        string stringToSign = $"{encodedResource}\n{expiry}";

        using var hmac = new HMACSHA256(Convert.FromBase64String(deviceKey));
        string signature = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(stringToSign)));
        string encodedSig = Uri.EscapeDataString(signature);

        return $"SharedAccessSignature sr={encodedResource}&sig={encodedSig}&se={expiry}";
    }

    public static bool IsSasTokenValid(string sasToken)
    {
        if (string.IsNullOrWhiteSpace(sasToken)) return false;

        foreach (var p in sasToken.Split('&', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (p.StartsWith("se=", StringComparison.OrdinalIgnoreCase) &&
                long.TryParse(p.AsSpan(3), out long se))
            {
                long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                return now < se - 60;
            }
        }
        return false;
    }
}