using Azure.Storage.Blobs;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing.Charts;
using IBS_Backend.Sanitary;
using MMS.Adapter.AzureRedisCache.BackgroundService;
using MMS.Adapter.AzureRedisCache.Extensions;
using MMS.Adapter.MQTT.Listener;
using MMS.Adapter.Reports.Extensions;
using MMS.Adapters.AdapterTwilio;
using MMS.Application.Ports.In.MachineSensor.Validator;
using System.Diagnostics;
using System.IO.Compression;
using Microsoft.AspNetCore.ResponseCompression;
using MMS.WebAPI.Serilog;
using MMS.WebAPI.Middleware;
using Serilog.Formatting.Compact;

var builder = WebApplication.CreateBuilder(args);

// ========== LOGGING ==========
// Configure LoggingFeaturesOptions from appsettings
builder.Services.Configure<LoggingFeaturesOptions>(builder.Configuration.GetSection("LoggingFeatures"));

// Configure Serilog with database sink
// Note: Database sink will use service provider that's built after services are registered
builder.Host.UseSerilog((ctx, lc) =>
{
    lc.ReadFrom.Configuration(ctx.Configuration);

    // Create a temporary service provider to pass to the sink
    // The sink will create its own scopes when needed
    var tempServices = builder.Services.BuildServiceProvider();
    var formatter = new CompactJsonFormatter();
    lc.WriteTo.Sink(new DatabaseLogSink(formatter, tempServices),
        restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Warning);
});

builder.Logging.AddSentry(o =>
{
    o.Dsn = builder.Configuration["Sentry:Dsn"];
    o.Environment = builder.Configuration["Sentry:Environment"];
    o.TracesSampleRate = builder.Configuration.GetValue<double>("Sentry:TracesSampleRate");
    o.MinimumBreadcrumbLevel = LogLevel.Information;
    o.MinimumEventLevel = LogLevel.Warning;
    o.MaxBreadcrumbs = 200;
    o.ReportAssembliesMode = ReportAssembliesMode.None;
});

builder.WebHost.UseSentry();

// ========== CONFIGURATION ==========
builder.Services.Configure<BlobStorageOptions>(builder.Configuration.GetSection("BlobStorage"));
builder.Services.Configure<RedisCacheOptions>(builder.Configuration.GetSection("RedisCache"));
builder.Services.Configure<StripeOptions>(builder.Configuration.GetSection("Stripe"));
builder.Services.Configure<FirebaseConfig>(builder.Configuration.GetSection("FirebaseConfig"));
builder.Services.Configure<EmailSetting>(builder.Configuration.GetSection("EmailSetting"));
builder.Services.Configure<StorageSetting>(builder.Configuration.GetSection("StorageSetting"));
builder.Services.Configure<HistoricalStatsJobOptions>(builder.Configuration.GetSection("Jobs:HistoricalStatsJob"));
builder.Services.Configure<ScheduledReportJobOptions>(builder.Configuration.GetSection("Jobs:ScheduledReportJob"));

// ========== EXTERNAL SERVICES ==========
// Azure Blob Storage
builder.Services.AddSingleton(sp =>
{
    var options = sp.GetRequiredService<IOptions<BlobStorageOptions>>().Value;
    return new BlobServiceClient(options.ConnectionString);
});

// Azure Redis Cache
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var options = sp.GetRequiredService<IOptions<RedisCacheOptions>>().Value;
    var configuration = ConfigurationOptions.Parse(options.ConnectionString);
    configuration.AbortOnConnectFail = false; // ✅ Better resilience
    return ConnectionMultiplexer.Connect(configuration);
});
builder.Services.AddRedisCacheServices(); // ✅ SOLID: Registers all Redis services with proper abstractions
builder.Services.AddDistributedLockService();

// Stripe
builder.Services.AddSingleton<StripeClient>(sp =>
{
    var secretKey = builder.Configuration["Stripe:SecretKey"];
    return new StripeClient(secretKey);
});

// ========== APPLICATION SERVICES ==========
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddConfiguredControllers();
builder.Services.AddCorsPolicy();
builder.Services.AddSwagger();
builder.Services.AddRestApplicationServices();

// ✅ OPTIMIZATION: Response Compression (gzip/brotli)
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
    {
        "application/json",
        "application/xml",
        "text/json",
        "text/xml"
    });
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Optimal;
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Optimal;
});

// ✅ OPTIMIZATION: Memory Cache for static data
builder.Services.AddMemoryCache();

// Validation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CustomerDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<AddMachineSensorDtoValidator>();

// Database
builder.Services.AddDatabaseServices(builder.Configuration);
builder.Services.AddCosmosDatabaseServices(builder.Configuration);
builder.Services.AddCosmosClient(builder.Configuration);

// Authentication
builder.Services.AddKeyCloakServices(builder.Configuration);
builder.Services.AddKeycloakAuthentication(builder.Configuration).AddKeycloakAuthorization();

// AutoMapper
builder.Services.AddAutoMapper(typeof(ICustomerService), typeof(AutoMapperResult));

// ========== SCOPED SERVICES ==========
builder.Services.AddScoped<INotificationFirebaseService, NotificationFirebaseService>();
builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();
// ICacheService is now registered via AddRedisCacheServices() above
builder.Services.AddScoped<IScheduledReportService, ScheduledReportService>();

// ========== SINGLETON SERVICES ==========
builder.Services.AddSingleton<IRabbitMQProducer, RabbitMQProducer>();
builder.Services.AddSingleton<IConnectionMappingService, ConnectionMappingService>();

// ========== SIGNALR ==========
builder.Services.AddSignalR();

// ========== ADAPTERS ==========
builder.Services.AddTwilioAdapter(builder.Configuration);
builder.Services.AddEmailServices();
builder.Services.AddReportGenerator();

// ========== BACKGROUND SERVICES ==========
builder.Services.AddHostedService<RabbitMQConsumer>();
builder.Services.AddHostedService<MqttBackgroundListener>();
builder.Services.AddHostedService<RedisBackgroundService>();
builder.Services.AddHostedService<HistoricalStatsJob>();
builder.Services.AddHostedService<ScheduledReportJob>();
// ✅ OPTIMIZATION: Cache warming service (loads frequently accessed data on startup)
builder.Services.AddHostedService<CacheWarmingService>();

var app = builder.Build();

// ========== MIDDLEWARE PIPELINE (ORDER MATTERS!) ==========

// 1. Exception handling (must be first)
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// 2. Sentry middleware
app.UseMiddleware<SentryRequestResponseMiddleware>();

// 2.5. Database logging middleware (for HTTP request/response errors)
app.UseMiddleware<DatabaseLoggingMiddleware>();

// 3. Custom logging middleware
app.Use(async (ctx, next) =>
{
    var scopeVals = new Dictionary<string, object?>
    {
        ["hostName"] = Environment.MachineName,
        ["threadId"] = Environment.CurrentManagedThreadId,
        ["correlationId"] = Activity.Current?.Id ?? ctx.TraceIdentifier
    };
    using (app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Scope").BeginScope(scopeVals))
    {
        SentrySdk.ConfigureScope(s => s.SetTag("correlation_id", scopeVals["correlationId"]?.ToString() ?? ""));
        await next();
    }
});

// 4. Swagger (before routing)
app.UseSwagger(c =>
{
    c.RouteTemplate = "docs/{documentName}/swagger.json";
});

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/docs/v1/swagger.json", "MMS API v1");
    c.RoutePrefix = string.Empty;
});

// 5. Static files
app.UseStaticFiles();

// 6. Response Compression (must be before UseHttpsRedirection)
app.UseResponseCompression();

// 7. HTTPS redirect
app.UseHttpsRedirection();

// 8. Serilog request logging
app.UseSerilogRequestLogging();

// 9. Routing (MUST come before Auth)
app.UseRouting();

// 10. CORS (after routing, before auth)
app.UseCors("AllowAll");

// 11. Request source detection (before auth)
app.UseRequestSourceDetection();

// 12. Authentication (MUST come before Authorization)
app.UseAuthentication();

// 13. Authorization (MUST come after Authentication)
app.UseAuthorization();

// 14. Endpoints (MUST be last)
app.MapControllers();
app.MapHub<MachineHub>("/machineHub");

// 15. Run the application
app.Run();