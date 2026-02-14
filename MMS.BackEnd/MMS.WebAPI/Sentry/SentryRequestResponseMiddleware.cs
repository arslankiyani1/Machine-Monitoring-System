using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace IBS_Backend.Sanitary;

public class SentryRequestResponseMiddleware
{
    private readonly RequestDelegate _next;
    private readonly LoggingFeaturesOptions _opts;
    private readonly HashSet<string> _maskSet;

    public SentryRequestResponseMiddleware(RequestDelegate next, IOptions<LoggingFeaturesOptions> opts)
    {
        _next = next;
        _opts = opts.Value;
        _maskSet = new HashSet<string>(_opts.MaskKeys ?? new(), StringComparer.OrdinalIgnoreCase);
    }

    public async Task Invoke(HttpContext ctx)
    {
        if (_opts.ExcludePaths.Any(p => ctx.Request.Path.StartsWithSegments(p, StringComparison.OrdinalIgnoreCase)))
        { await _next(ctx); return; }
        if (_opts.SampleRate < 1.0 && Random.Shared.NextDouble() > _opts.SampleRate)
        { await _next(ctx); return; }

        var traceId = Activity.Current?.Id ?? ctx.TraceIdentifier;

        // ----- Request -----
        string reqBody = string.Empty;
        if (_opts.CaptureBodies)
        {
            ctx.Request.EnableBuffering();
            if (ctx.Request.ContentLength is > 0 && ctx.Request.Body.CanRead)
            {
                using var reader = new StreamReader(ctx.Request.Body, System.Text.Encoding.UTF8, leaveOpen: true);
                reqBody = await reader.ReadToEndAsync();
                ctx.Request.Body.Position = 0;
            }
        }

        var reqData = new Dictionary<string, string?>
        {
            ["method"] = ctx.Request.Method,
            ["path"] = ctx.Request.Path,
            ["query"] = ctx.Request.QueryString.ToString(),
            ["trace"] = traceId
        };

        if (_opts.IncludeHeaders)
        {
            var headers = ctx.Request.Headers.ToDictionary(k => k.Key, v => string.Join(",", v.Value), StringComparer.OrdinalIgnoreCase);
            MaskHeaders(headers, _maskSet);
            reqData["req_headers_masked"] = JsonSerializer.Serialize(headers);
        }

        if (_opts.CaptureBodies && !string.IsNullOrWhiteSpace(reqBody))
            reqData["req_body_masked"] = MaskJson(reqBody, _maskSet, _opts.MaxBodyChars);

        SentrySdk.AddBreadcrumb("HTTP request", category: "http.request", type: "request", data: reqData);

        // ----- Response -----
        var originalStream = ctx.Response.Body;
        await using var mem = new MemoryStream();
        ctx.Response.Body = mem;

        var sw = Stopwatch.StartNew();
        try { await _next(ctx); }
        finally
        {
            sw.Stop();
            mem.Position = 0;
            var resText = await new StreamReader(mem).ReadToEndAsync();
            mem.Position = 0;
            await mem.CopyToAsync(originalStream);
            ctx.Response.Body = originalStream;

            var resData = new Dictionary<string, string?>
            {
                ["status"] = ctx.Response.StatusCode.ToString(),
                ["duration_ms"] = sw.ElapsedMilliseconds.ToString(),
                ["path"] = ctx.Request.Path
            };

            if (_opts.IncludeHeaders)
            {
                var rheaders = ctx.Response.Headers.ToDictionary(k => k.Key, v => string.Join(",", v.Value), StringComparer.OrdinalIgnoreCase);
                MaskHeaders(rheaders, _maskSet);
                resData["res_headers_masked"] = JsonSerializer.Serialize(rheaders);
            }

            if (_opts.CaptureBodies && !string.IsNullOrWhiteSpace(resText))
                resData["res_body_masked"] = MaskJson(resText, _maskSet, _opts.MaxBodyChars);

            SentrySdk.AddBreadcrumb("HTTP response", category: "http.response", type: "response", data: resData);

            if (ctx.Response.StatusCode >= 400)
            {
                SentrySdk.ConfigureScope(s =>
                {
                    s.SetTag("http.status", ctx.Response.StatusCode.ToString());
                    s.SetTag("http.method", ctx.Request.Method);
                    s.SetTag("http.path", ctx.Request.Path);
                    s.SetExtra("duration_ms", sw.ElapsedMilliseconds);
                    s.SetTag("message_tag", "sanitary");
                    s.Level = ctx.Response.StatusCode >= 500 ? SentryLevel.Error : SentryLevel.Warning;
                });
                SentrySdk.CaptureMessage($"HTTP {(ctx.Response.StatusCode >= 500 ? "server" : "client")} error {ctx.Response.StatusCode} {ctx.Request.Method} {ctx.Request.Path}");
            }
        }
    }

    private static void MaskHeaders(IDictionary<string, string> headers, HashSet<string> maskSet)
    {
        foreach (var key in headers.Keys.ToList())
        {
            if (maskSet.Contains(key) || key.Equals("Authorization", StringComparison.OrdinalIgnoreCase) || key.Equals("Cookie", StringComparison.OrdinalIgnoreCase) || key.Equals("Set-Cookie", StringComparison.OrdinalIgnoreCase))
                headers[key] = "*";
        }
    }

    private static string MaskJson(string body, HashSet<string> maskSet, int max)
    {
        if (string.IsNullOrWhiteSpace(body)) return string.Empty;

        // 1) Hard cap
        var truncated = body.Length <= max ? body : body[..max] + "…";

        // 2) Try structured masking first
        try
        {
            var node = JsonNode.Parse(truncated);
            if (node is JsonObject obj) { MaskObject(obj, maskSet); return obj.ToJsonString(); }
            if (node is JsonArray arr)
            {
                foreach (var el in arr)
                    if (el is JsonObject o) MaskObject(o, maskSet);
                return arr.ToJsonString();
            }
        }
        catch
        {
            // fall through
        }

        // 3) Regex fallback for loose JSON / plain text
        foreach (var key in maskSet)
        {
            var k = System.Text.RegularExpressions.Regex.Escape(key);
            truncated = System.Text.RegularExpressions.Regex.Replace(
                truncated,
                $"(?i)(\"{k}\"\\s*:\\s*\")([^\"]*)(\")",
                "$1***$3");
            truncated = System.Text.RegularExpressions.Regex.Replace(
                truncated,
                $"(?i)('{k}'\\s*:\\s*')([^']*)(')",
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