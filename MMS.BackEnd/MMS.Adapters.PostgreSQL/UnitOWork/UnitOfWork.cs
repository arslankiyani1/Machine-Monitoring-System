using MMS.Application.Ports.In.NoSql.MachineSensorData;
using MMS.Application.Ports.Out.Persistence.Interfaces.NoSql;

namespace MMS.Adapters.PostgreSQL.UnitOWork;

public class UnitOfWork(

    ApplicationDbContext context,
    ICustomerRepository customerRepository,
    IMachineRepository machineRepository,
    IWidgetRepository widgetRepository,
    ICustomerDashboardRepository customerDashboardRepository,
    INotificationRepository notificationRepository,
    IMachineSettingRepository machineSettingRepository,
    ICustomerReportSettingRepository customerReportSettingRepository,
    ICustomerSubscriptionRepository customerSubscriptionRepository,
    IMachineMaintenanceTaskRepository machineMaintenanceTaskRepository,
    ISubscriptionRepository subscriptionRepository,
    IUserMachineRepository userMachineRepository,
    ISupportRepository supportRepository,
    IInvoiceRepository invoiceRepository,
    IMachineSensorRepository machineSensorRepository,
    ICustomerReportRepository customerReportRepository,
    IMachineSensorLogRepository machineSensorLog
    ) : IUnitOfWork
{
    private readonly ApplicationDbContext dbContext = context;
    private IDbContextTransaction? _currentTransaction;

    public ICustomerRepository CustomerRepository { get; } = customerRepository;
    public IMachineRepository MachineRepository { get; } = machineRepository;
    public ICustomerDashboardRepository CustomerDashboardRepository { get; } = customerDashboardRepository;
    public IWidgetRepository WidgetRepository { get; } = widgetRepository;
    public ICustomerReportSettingRepository CustomerReportSettingRepository { get; } = customerReportSettingRepository;
    public IMachineSettingRepository MachineSettingRepository { get; } = machineSettingRepository;
    public INotificationRepository NotificationRepository { get; } = notificationRepository;
    public ICustomerSubscriptionRepository CustomerSubscriptionRepository { get; } = customerSubscriptionRepository;
    public IMachineMaintenanceTaskRepository MachineMaintenanceTaskRepository { get; } = machineMaintenanceTaskRepository;
    public ISubscriptionRepository SubscriptionRepository {  get; } = subscriptionRepository;
    public IUserMachineRepository UserMachineRepository {  get; } = userMachineRepository;
    public ISupportRepository SupportRepository { get; } = supportRepository;
    public IInvoiceRepository InvoiceRepository { get; } = invoiceRepository;
    public IMachineSensorRepository MachineSensorRepository { get; } = machineSensorRepository;
    public IMachineSensorLogRepository MachineSensorLogRepository { get; } = machineSensorLog;
    public ICustomerReportRepository CustomerReportRepository { get; } = customerReportRepository;

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _currentTransaction ??= await dbContext.Database.BeginTransactionAsync(cancellationToken);
        return _currentTransaction;
    }

    public async Task<int> SaveChangesAsync()
    {
        try
        {
            foreach (var entry in dbContext.ChangeTracker.Entries().ToList())
            {
                if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
                {
                    var properties = entry.Properties
                        .Where(p => p.Metadata.ClrType == typeof(DateTimeOffset) ||
                                    p.Metadata.ClrType == typeof(DateTimeOffset?)).ToList();
                    foreach (var property in properties)
                    {
                        if (property.CurrentValue is DateTimeOffset dtoValue)
                        {
                            property.CurrentValue = dtoValue.ToUniversalTime();
                        }
                    }
                }
            }
            return await dbContext.SaveChangesAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }
}