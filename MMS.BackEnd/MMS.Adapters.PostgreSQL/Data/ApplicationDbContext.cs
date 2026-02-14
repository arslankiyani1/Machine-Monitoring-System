namespace MMS.Adapters.PostgreSQL.Data;

public class ApplicationDbContext : DbContext
{
    private readonly ILogger<ApplicationDbContext> _logger;
    private readonly Guid? _userId;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ILogger<ApplicationDbContext> logger,
        IUserContextService userContextService
        )
        : base(options)
    {
        _logger = logger;
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        ChangeTracker.LazyLoadingEnabled = false;
        _userId = userContextService.UserId;
    }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<CustomerDashboard> CustomerDashboards { get; set; }
    public DbSet<CustomerReportSetting> CustomerReportSetting { get; set; }
    public DbSet<CustomerSubscription> CustomerSubscriptions { get; set; }
    public DbSet<Machine> Machines { get; set; }
    public DbSet<MachineMaintenanceTask> MachineMaintenances { get; set; }
    public DbSet<MachineSetting> MachineSettings { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<Subscription> Subscriptions { get; set; }
    public DbSet<ApplicationLog> ApplicationLogs { get; set; }
    public DbSet<UserMachine> UserMachines { get; set; }
    public DbSet<Widget> Widgets { get; set; }
    public DbSet<Support> Supports { get; set; }
    public DbSet<Invoice> invoices { get; set; }
    public DbSet<CustomerBillingAddress> customerBillingAddresses { get; set; }
    public DbSet<CustomerReport> CustomerReports { get; set; }
    public DbSet<MachineSensor> MachineSensor { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        DbSeed.SeedDatabase(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.ConfigureWarnings(warnings =>
            warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        HandleTrackingProperties();
        try
        {
            int result = await base.SaveChangesAsync(cancellationToken);
            if (result < 1)
            {
                _logger.LogWarning("No entity is added to the database {@result}", result);
            }
            return result;
        }
        catch (DbUpdateException e) when (e.InnerException is PostgresException)
        {
            _logger.LogError(e, "An error occurred while updating the database.");
            throw;
        }
    }

    private void HandleTrackingProperties()
    {
        foreach (var entry in ChangeTracker.Entries<ITrackable>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.CurrentValues[nameof(ITrackable.CreatedAt)] = DateTime.UtcNow;
                entry.CurrentValues[nameof(ITrackable.CreatedBy)] = _userId;
                entry.CurrentValues[nameof(ITrackable.UpdatedAt)] = DateTime.UtcNow;
                entry.CurrentValues[nameof(ITrackable.UpdatedBy)] = _userId;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.CurrentValues[nameof(ITrackable.UpdatedAt)] = DateTime.UtcNow;
                entry.CurrentValues[nameof(ITrackable.UpdatedBy)] = _userId;
            }
        }
        foreach (var entry in ChangeTracker.Entries<ISoftDelete>())
        {
            if (entry.Entity is UserMachine)
                continue;
            if (entry.Entity is Customer)
                continue;
            if (entry.Entity is Machine)
                continue;
            if (entry.Entity is MachineSetting)
                continue;
            if (entry.Entity is Support)
                continue;
            if (entry.Entity is MachineSensor)
                continue; 
            if (entry.Entity is MachineMaintenanceTask)
                continue;
            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.CurrentValues[nameof(ISoftDelete.Deleted)] = DateTime.UtcNow;
                entry.CurrentValues[nameof(ISoftDelete.DeletedBy)] = _userId;
            }
        }
    }
}