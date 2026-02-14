using Microsoft.AspNetCore.Builder;
using MMS.Application.Common.Constants;

namespace MMS.Adapters.PostgreSQL.CustomMiddleware;

public class RequestSourceMiddleware
{
    private readonly RequestDelegate _next;

    public RequestSourceMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Detect and store source in HttpContext Items
        var source = RequestSourceDetector.DetectSource(context);
        context.Items["RequestSource"] = source;

        await _next(context);
    }
}

// Extension method for easy registration
public static class RequestSourceMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestSourceDetection(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestSourceMiddleware>();
    }
}
