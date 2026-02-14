namespace MMS.Adapters.NoSQL.Configuration;

public class CustomerDashboardSummaryConfiguration : IEntityTypeConfiguration<CustomerDashboardSummary>
{
    public void Configure(EntityTypeBuilder<CustomerDashboardSummary> builder)
    {
        builder.ToContainer(Constant.CustomerDashboardSummary);
        builder.HasPartitionKey(e => e.CustomerId);
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).ToJsonProperty("id");
        builder.Property(e => e.CustomerId).ToJsonProperty("customer_id");
        builder.Property(e => e.StatusSummary).ToJsonProperty("status_summary");
        builder.Property(e => e.CreatedAt).ToJsonProperty("created_at");
        builder.Property(e => e.UpdatedAt).ToJsonProperty("updated_at");

        builder.HasNoDiscriminator();
    }
}