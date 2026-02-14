//namespace MMS.Application.Common.CustomMiddleware;

//using System.Diagnostics;
//using Microsoft.AspNetCore.Http;
//using Microsoft.Extensions.Logging;

//public class SlowRequestLoggingMiddleware
//{
//    private readonly RequestDelegate _next;
//    private readonly ILogger<SlowRequestLoggingMiddleware> _logger;
//    private const int ThresholdMilliseconds = 2000;

//    public SlowRequestLoggingMiddleware(RequestDelegate next, ILogger<SlowRequestLoggingMiddleware> logger)
//    {
//        _next = next;
//        _logger = logger;
//    }

//    public async Task Invoke(HttpContext context)
//    {
//        var stopwatch = Stopwatch.StartNew();

//        await _next(context);

//        stopwatch.Stop();

//        if (stopwatch.ElapsedMilliseconds > ThresholdMilliseconds)
//        {
//            var ip = context.Connection?.RemoteIpAddress?.ToString() ?? "unknown";

//            _logger.LogWarning("Slow request detected {@RequestDetails}",
//                new
//                {
//                    Method = context.Request.Method,
//                    Path = context.Request.Path,
//                    ElapsedMs = stopwatch.ElapsedMilliseconds,
//                    IP = ip,
//                    StatusCode = context.Response.StatusCode
//                });
//        }
//    }
//}
