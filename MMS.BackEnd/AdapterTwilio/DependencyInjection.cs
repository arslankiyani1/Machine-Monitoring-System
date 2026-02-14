using AdapterTwilio;
using AdapterTwilio.Services;
using AdapterTwilio.Abstractions;
using AdapterTwilio.Implementations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MMS.Application.Interfaces;

namespace MMS.Adapters.AdapterTwilio;

/// <summary>
/// Dependency injection extensions following SOLID principles
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddTwilioAdapter(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Bind settings
        services.Configure<TwilioSettings>(
            configuration.GetSection(TwilioSettings.SectionName));

        // Register abstractions and implementations (Dependency Inversion Principle)
        services.AddSingleton<ITwilioClientWrapper, TwilioClientWrapper>();
        services.AddSingleton<IPhoneNumberNormalizer, PhoneNumberNormalizer>();
        services.AddSingleton<ITwilioErrorMapper, TwilioErrorMapper>();
        services.AddSingleton<IMessageTemplateService, MessageTemplateService>();
        services.AddSingleton<IRetryPolicyFactory, RetryPolicyFactory>();

        // Register port (adapter) - composed of specialized services
        services.AddSingleton<ISmsNotificationPort, TwilioSmsAdapter>();

        // Register application service
        services.AddScoped<ISmsNotificationService, SmsNotificationService>();

        return services;
    }
}