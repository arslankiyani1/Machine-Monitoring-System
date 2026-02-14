using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using MMS.Adapters.PostgreSQL.Data;
using MMS.Application.Models.SQL;
using Microsoft.Extensions.Options;
using IBS_Backend.Sanitary;

namespace MMS.WebAPI.Middleware;

public class DatabaseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<DatabaseLoggingMiddleware> _logger;
    private readonly LoggingFeaturesOptions _opts;
    private readonly HashSet<string> _maskSet;

    public DatabaseLoggingMiddleware(
        RequestDelegate next,
        ILogger<DatabaseLoggingMiddleware> logger,
        IOptions<LoggingFeaturesOptions> opts)
    {
        _next = next;
        _logger = logger;
        _opts = opts.Value;
        _maskSet = new HashSet<string>(_opts.MaskKeys ?? new(), StringComparer.OrdinalIgnoreCase);
    }

    public async Task InvokeAsync(HttpContext context, IServiceProvider serviceProvider)
    {
        // Skip excluded paths
        if (_opts.ExcludePaths.Any(p => context.Request.Path.StartsWithSegments(p, StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context);
            return;
        }

        // Apply sampling rate
        if (_opts.SampleRate < 1.0 && Random.Shared.NextDouble() > _opts.SampleRate)
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        string? requestBody = null;
        string? responseBody = null;

        // Capture request body
        if (_opts.CaptureBodies && context.Request.ContentLength > 0)
        {
            context.Request.EnableBuffering();
            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
            requestBody = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;
        }

        // Capture response body
        var originalBodyStream = context.Response.Body;
        using var responseBodyStream = new MemoryStream();
        context.Response.Body = responseBodyStream;

        Exception? exception = null;
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            exception = ex;
            throw;
        }
        finally
        {
            stopwatch.Stop();

            // Capture response body
            if (_opts.CaptureBodies)
            {
                responseBodyStream.Seek(0, SeekOrigin.Begin);
                responseBody = await new StreamReader(responseBodyStream).ReadToEndAsync();
                responseBodyStream.Seek(0, SeekOrigin.Begin);
                await responseBodyStream.CopyToAsync(originalBodyStream);
            }
            else
            {
                await responseBodyStream.CopyToAsync(originalBodyStream);
            }
            context.Response.Body = originalBodyStream;

            // Log to database for errors and important requests
            if (context.Response.StatusCode >= 400 || exception != null)
            {
                try
                {
                    using var scope = serviceProvider.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    // Build full URL
                    var fullUrl = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}";

                    var log = new ApplicationLog
                    {
                        Id = Guid.NewGuid(),
                        Level = context.Response.StatusCode >= 500 ? "Error" : "Warning",
                        Message = $"HTTP {context.Response.StatusCode} {context.Request.Method} {context.Request.Path}",
                        Exception = exception?.ToString(),
                        Url = fullUrl
                        // CreatedAt, UpdatedAt, CreatedBy, UpdatedBy are automatically set by ApplicationDbContext
                    };

                    // Build request data
                    var requestData = new Dictionary<string, object?>
                    {
                        ["method"] = context.Request.Method,
                        ["path"] = context.Request.Path.ToString(),
                        ["queryString"] = context.Request.QueryString.ToString(),
                        ["elapsedMs"] = stopwatch.ElapsedMilliseconds
                    };

                    if (_opts.IncludeHeaders)
                    {
                        var headers = context.Request.Headers
                            .ToDictionary(k => k.Key, v => string.Join(",", v.Value.ToArray()), StringComparer.OrdinalIgnoreCase);
                        MaskHeaders(headers, _maskSet);
                        requestData["headers"] = headers;
                    }

                    if (!string.IsNullOrWhiteSpace(requestBody))
                    {
                        requestData["body"] = MaskJson(requestBody, _maskSet, _opts.MaxBodyChars);
                    }

                    log.ApiRequest = JsonSerializer.Serialize(requestData);

                    // Build response data
                    var responseData = new Dictionary<string, object?>
                    {
                        ["statusCode"] = context.Response.StatusCode,
                        ["elapsedMs"] = stopwatch.ElapsedMilliseconds
                    };

                    if (_opts.IncludeHeaders)
                    {
                        var headers = context.Response.Headers
                            .ToDictionary(k => k.Key, v => string.Join(",", v.Value.ToArray()), StringComparer.OrdinalIgnoreCase);
                        MaskHeaders(headers, _maskSet);
                        responseData["headers"] = headers;
                    }

                    if (!string.IsNullOrWhiteSpace(responseBody))
                    {
                        responseData["body"] = MaskJson(responseBody, _maskSet, _opts.MaxBodyChars);
                    }

                    log.ApiResponse = JsonSerializer.Serialize(responseData);

                    dbContext.ApplicationLogs.Add(log);
                    await dbContext.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    // Log to Serilog instead if database fails
                    _logger.LogError(ex, "Failed to write log to database");
                }
            }
        }
    }

    private static void MaskHeaders(IDictionary<string, string> headers, HashSet<string> maskSet)
    {
        foreach (var key in headers.Keys.ToList())
        {
            if (maskSet.Contains(key) || 
                key.Equals("Authorization", StringComparison.OrdinalIgnoreCase) || 
                key.Equals("Cookie", StringComparison.OrdinalIgnoreCase) || 
                key.Equals("Set-Cookie", StringComparison.OrdinalIgnoreCase))
            {
                headers[key] = "*";
            }
        }
    }

    private static string MaskJson(string body, HashSet<string> maskSet, int max)
    {
        if (string.IsNullOrWhiteSpace(body)) return string.Empty;

        var truncated = body.Length <= max ? body : body[..max] + "…";

        try
        {
            var node = JsonNode.Parse(truncated);
            if (node is JsonObject obj)
            {
                MaskObject(obj, maskSet);
                return obj.ToJsonString();
            }
            if (node is JsonArray arr)
            {
                foreach (var el in arr)
                    if (el is JsonObject o) MaskObject(o, maskSet);
                return arr.ToJsonString();
            }
        }
        catch
        {
            // Fall through to regex masking
        }

        // Regex fallback
        foreach (var key in maskSet)
        {
            var k = System.Text.RegularExpressions.Regex.Escape(key);
            truncated = System.Text.RegularExpressions.Regex.Replace(
                truncated,
                $"(?i)(\"{k}\"\\s*:\\s*\")([^\"]*)(\")",
                "$1***$3");
        }

        return truncated;
    }

    private static void MaskObject(JsonObject obj, HashSet<string> maskSet)
    {
        foreach (var kv in obj.ToList())
        {
            if (kv.Value is JsonObject child)
                MaskObject(child, maskSet);
            else if (kv.Value is JsonArray arr)
            {
                for (int i = 0; i < arr.Count; i++)
                    if (arr[i] is JsonObject o) MaskObject(o, maskSet);
            }
            else if (maskSet.Contains(kv.Key))
                obj[kv.Key] = "*";
        }
    }
}
