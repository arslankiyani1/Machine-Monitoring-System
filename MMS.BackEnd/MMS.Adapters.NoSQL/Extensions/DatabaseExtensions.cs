using MMS.Application.Common;
using MMS.Application.Services.NoSql;

namespace MMS.Adapters.NoSQL.Extensions;

public static class DatabaseExtensions
{
    public static void AddCosmosDatabaseServices(this IServiceCollection services, IConfiguration configuration)
    {
        var endpoint = configuration["Cosmos:EndpointUri"] ?? throw new InvalidOperationException("Cosmos endpoint URI is not configured.");
        var key = configuration["Cosmos:PrimaryKey"] ?? throw new InvalidOperationException("Cosmos primary key is not configured.");
        var dbName = configuration["Cosmos:DatabaseName"] ?? throw new InvalidOperationException("Cosmos database name is not configured.");

        services.AddDbContext<MyCosmosDbContext>(options =>
        options.UseCosmos(endpoint, key, dbName, cosmosOptions =>
        {
            // Connection timeout settings to prevent I/O operation aborted exceptions
            cosmosOptions.RequestTimeout(TimeSpan.FromSeconds(60));
            cosmosOptions.OpenTcpConnectionTimeout(TimeSpan.FromSeconds(30));
            cosmosOptions.IdleTcpConnectionTimeout(TimeSpan.FromMinutes(10));
        }),
        ServiceLifetime.Scoped); // explicit, for clarity

        services.AddScoped<IMachineStatusSettingRepository, MachineStatusSettingRepository>();
        services.AddScoped<IMachineJobRepository, MachineJobRepository>();
        services.AddScoped<IMachineSensorLogRepository, MachineSensorDataRepository>();
        services.AddScoped<IMachineLogRepository, MachineLogRepository>();
        services.AddScoped<IHistoricalStatsRepository, HistoricalStatsRepository>();
        services.AddScoped<IAlertRuleRepository, AlertRuleRepository>();
        services.AddScoped<ICustomerDashboardSummaryRepository, CustomerDashboardSummaryRepository>();
        services.AddScoped<IAlertRepository, AlertRepository>();
        services.AddScoped<IOperationalDataRepository, OperationalDataRepository>();
        services.AddScoped<IAlertEvaluationService, AlertRuleEvaluator>();
    }
}