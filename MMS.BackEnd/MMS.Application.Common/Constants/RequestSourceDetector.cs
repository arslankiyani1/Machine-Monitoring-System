using Microsoft.AspNetCore.Http;

namespace MMS.Application.Common.Constants;

public static class RequestSourceDetector
{
    public static string DetectSource(HttpContext? httpContext)
    {
        if (httpContext == null)
            return "System";

        var request = httpContext.Request;

        // Check for custom header first (highest priority)
        if (request.Headers.TryGetValue("X-Source", out var customSource))
        {
            return customSource.ToString();
        }

        // Check User-Agent header
        var userAgent = request.Headers["User-Agent"].ToString().ToLower();

        // Detect Postman
        if (userAgent.Contains("postman"))
            return "Postman";

        // Detect Swagger/OpenAPI
        if (userAgent.Contains("swagger") ||
            request.Path.StartsWithSegments("/swagger") ||
            request.Headers["Referer"].ToString().Contains("/swagger"))
            return "Swagger";

        // Detect Insomnia
        if (userAgent.Contains("insomnia"))
            return "Insomnia";

        // Detect Thunder Client (VS Code)
        if (userAgent.Contains("thunder client"))
            return "ThunderClient";

        // Detect cURL
        if (userAgent.Contains("curl"))
            return "cURL";

        // Detect MQTT (if it comes through HTTP bridge)
        if (request.Headers.ContainsKey("X-MQTT-Topic") ||
            userAgent.Contains("mqtt"))
            return "MQTT";

        // Detect SignalR
        if (request.Path.StartsWithSegments("/hubs") ||
            request.Headers.ContainsKey("X-SignalR-User-Agent"))
            return "SignalR";

        // Detect ESP32/IoT devices
        if (userAgent.Contains("esp32") ||
            userAgent.Contains("arduino") ||
            request.Path.StartsWithSegments("/api/esp32"))
            return "ESP32";

        // Detect Browser-based requests
        if (userAgent.Contains("mozilla") ||
            userAgent.Contains("chrome") ||
            userAgent.Contains("safari") ||
            userAgent.Contains("edge"))
            return "Browser";

        // Detect Mobile Apps
        if (userAgent.Contains("okhttp") ||
            userAgent.Contains("cfnetwork") ||
            userAgent.Contains("alamofire"))
            return "MobileApp";

        // Check Content-Type for API clients
        var contentType = request.ContentType?.ToLower() ?? string.Empty;
        if (contentType.Contains("application/json"))
            return "API";

        // Default fallback
        return "Unknown";
    }
}
