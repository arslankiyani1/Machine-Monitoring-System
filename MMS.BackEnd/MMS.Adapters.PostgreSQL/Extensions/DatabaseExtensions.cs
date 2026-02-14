namespace MMS.Adapters.PostgreSQL.Extensions;

public static class DatabaseExtensions
{
    public static void AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
    {
        var dataSource = new NpgsqlDataSourceBuilder(configuration.GetConnectionString("DefaultConnection"))
                .EnableDynamicJson()
                .Build();

        services.AddSingleton(dataSource);
        services.AddDbContext<ApplicationDbContext>((sp, options) => options
                .UseNpgsql(sp.GetRequiredService<NpgsqlDataSource>())
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors());

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IMachineRepository, MachineRepository>();
        services.AddScoped<IWidgetRepository, WidgetRepository>();
        services.AddScoped<ICustomerDashboardRepository, CustomerDashboardRepository>();
        services.AddScoped<IMachineSettingRepository, MachineSettingRepository>();
        services.AddScoped<ICustomerReportSettingRepository, CustomerReportSettingRepository>();
        services.AddScoped<ICustomerReportRepository, CustomerReportRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<ICustomerSubscriptionRepository, CustomerSubscriptionRepository>();
        services.AddScoped<IMachineMaintenanceTaskRepository, MachineMaintenanceTaskRepository>();
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<IUserMachineRepository, UserMachineRepository>();
        services.AddScoped<ISupportRepository, SupportRepository>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<ICustomerBillingAddressRepository, CustomerBillingAddressRepository>();
        services.AddScoped<IMachineSensorRepository, MachineSensorRepository>();
    }
}