namespace MMS.Adapters.Email.Extensions;

[ExcludeFromCodeCoverage]
public static class EmailExtensions
{
    public static IServiceCollection AddEmailServices(this IServiceCollection services)
    {
        services.AddSingleton(_ => Channel.CreateUnbounded<Func<Task>>());
        services.AddSingleton<IEmailQueueService, EmailQueueService>();
        services.AddHostedService<EmailBackgroundService>();
        services.AddTransient<IEmailService, EmailService>();
        return services;
    }
}