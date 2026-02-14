namespace MMS.Adapters.NoSQL.Data;

public class MyCosmosDbContext(
    DbContextOptions<MyCosmosDbContext> options) : DbContext(options)
{
    public DbSet<MachineStatusSetting> MachineStatusSettings { get; set; }
    public DbSet<MachineJob> MachineJobs { get; set; }
    public DbSet<MachineSensorLog> MachineSensorData { get; set; }
    public DbSet<MachineLog> MachineLogs { get; set; }
    public DbSet<HistoricalStats> HistoricalStats { get; set; }
    public DbSet<AlertRule> AlertRule { get; set; }
    public DbSet<Alert> Alerts { get; set; }
    public DbSet<OperationalData> OperationalData { get; set; }
    public DbSet<CustomerDashboardSummary> CustomerDashboardSummaries { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}