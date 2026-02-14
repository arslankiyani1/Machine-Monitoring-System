namespace MMS.Application.Ports.Out.Persistence;

public interface IUnitOfWork
{
    ICustomerRepository CustomerRepository { get; }
    IMachineRepository MachineRepository { get; }
    IWidgetRepository WidgetRepository { get; }
    ICustomerReportSettingRepository CustomerReportSettingRepository { get; }
    ICustomerReportRepository CustomerReportRepository { get;  }
    IMachineSettingRepository MachineSettingRepository { get; }
    ICustomerDashboardRepository CustomerDashboardRepository { get; }
    INotificationRepository NotificationRepository { get; }
    ICustomerSubscriptionRepository CustomerSubscriptionRepository { get; }
    IMachineMaintenanceTaskRepository MachineMaintenanceTaskRepository { get; }
    ISubscriptionRepository SubscriptionRepository { get; }
    IUserMachineRepository UserMachineRepository { get; }
    ISupportRepository SupportRepository { get; }
    IInvoiceRepository InvoiceRepository { get; }
    IMachineSensorRepository MachineSensorRepository { get; }
    IMachineSensorLogRepository MachineSensorLogRepository { get; }

    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync();
}