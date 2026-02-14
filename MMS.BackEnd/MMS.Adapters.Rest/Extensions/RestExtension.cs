using MMS.Application.Ports.In.MachineSensor;
using MMS.Application.Ports.In.NoSql.CustomerDashSummary;
using MMS.Application.Services.NoSql;

namespace MMS.Adapters.Rest.Extensions;

[ExcludeFromCodeCoverage]
public static class RestExtension
{
    public static void AddRestApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<AutoMapperResult>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IMachineService, MachineService>();
        services.AddScoped<ICustomerReportSettingService, CustomerReportSettingService>();
        services.AddScoped<ICustomerReportService, CustomerReportService>();
        services.AddScoped<IMachineSettingService, MachineSettingService>();
        services.AddScoped<ICustomerDashboardService, CustomerDashboardService>();
        services.AddScoped<IWidgetService, WidgetService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<ICustomerSubscriptionService, CustomerSubscriptionService>();
        services.AddScoped<IMachineMaintenanceTaskService, MachineMaintenanceTaskService>();
        services.AddScoped<ISubscriptionService, SubscriptionService>();
        services.AddScoped<IUserMachineService, UserMachineService>();
        services.AddScoped<IMachineStatusSettingService, MachineStatusSettingService>();
        services.AddScoped<IMachineJobService, MachineJobService>();
        services.AddScoped<IMachineSensorLogService, MachineSensorLogService>();
        services.AddScoped<IMachineLogService, MachineLogService>();
        services.AddScoped<IMachineMonitoringService, MachineMonitoringService>();
        services.AddScoped<ISupportService, SupportService>();
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<ICustomerBillingAddressService, CustomerBillingAddressService>();
        services.AddScoped<IHistoricalStatsService, HistoricalStatsService>();
        services.AddScoped<IAlertRuleService, AlertRuleService>();
        services.AddScoped<ICustomerDashboardSummaryService, CustomerDashboardSummaryService>();
        services.AddScoped<IMachineSensorService, MachineSensorService>();
        services.AddScoped<IAlertNotificationService, AlertNotificationService>();

    }
}