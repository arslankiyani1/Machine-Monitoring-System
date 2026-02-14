using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Microsoft.Extensions.DependencyInjection;
using MMS.Adapters.PostgreSQL.Data;
using MMS.Application.Models.SQL;
using System.Text.Json;

namespace MMS.WebAPI.Serilog;

public class DatabaseLogSink : ILogEventSink
{
    private readonly ITextFormatter _formatter;
    private readonly IServiceProvider _serviceProvider;

    public DatabaseLogSink(ITextFormatter formatter, IServiceProvider serviceProvider)
    {
        _formatter = formatter;
        _serviceProvider = serviceProvider;
    }

    public void Emit(LogEvent logEvent)
    {
        try
        {
            // Use a background task to avoid blocking the logging pipeline
            _ = Task.Run(async () =>
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    var log = new ApplicationLog
                    {
                        Id = Guid.NewGuid(),
                        Level = logEvent.Level.ToString(),
                        Message = logEvent.RenderMessage(),
                        Exception = logEvent.Exception?.ToString()
                        // CreatedAt, UpdatedAt, CreatedBy, UpdatedBy are automatically set by ApplicationDbContext
                    };

                    // Extract URL and request properties if available
                    if (logEvent.Properties.TryGetValue("RequestPath", out var requestPath))
                    {
                        var path = requestPath.ToString().Trim('"');
                        
                        // Get query string once
                        var queryStringValue = logEvent.Properties.TryGetValue("QueryString", out var queryStringProp) 
                            ? queryStringProp.ToString().Trim('"') 
                            : string.Empty;
                        
                        // Build full URL
                        if (logEvent.Properties.TryGetValue("RequestScheme", out var scheme) &&
                            logEvent.Properties.TryGetValue("RequestHost", out var host))
                        {
                            var schemeValue = scheme.ToString().Trim('"');
                            var hostValue = host.ToString().Trim('"');
                            
                            log.Url = $"{schemeValue}://{hostValue}{path}{queryStringValue}";
                        }
                        else
                        {
                            // Fallback to just path if scheme/host not available
                            log.Url = path;
                        }

                        // Build ApiRequest JSON
                        log.ApiRequest = JsonSerializer.Serialize(new
                        {
                            Path = path,
                            Method = logEvent.Properties.TryGetValue("RequestMethod", out var method) 
                                ? method.ToString().Trim('"') 
                                : null,
                            QueryString = !string.IsNullOrEmpty(queryStringValue) ? queryStringValue : null
                        });
                    }

                    if (logEvent.Properties.TryGetValue("StatusCode", out var statusCode))
                    {
                        log.ApiResponse = JsonSerializer.Serialize(new
                        {
                            StatusCode = statusCode.ToString().Trim('"'),
                            Elapsed = logEvent.Properties.TryGetValue("ElapsedMilliseconds", out var elapsed) 
                                ? elapsed.ToString().Trim('"') 
                                : null
                        });
                    }

                    // Only log Warning and above to database to avoid flooding
                    if (logEvent.Level >= LogEventLevel.Warning)
                    {
                        dbContext.ApplicationLogs.Add(log);
                        await dbContext.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    // Silently fail to avoid logging recursion
                    // Could optionally write to a file or console
                    System.Diagnostics.Debug.WriteLine($"Failed to write log to database: {ex.Message}");
                }
            });
        }
        catch
        {
            // Silently fail to avoid breaking the application
        }
    }
}
