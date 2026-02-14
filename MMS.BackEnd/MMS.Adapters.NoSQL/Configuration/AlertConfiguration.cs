namespace MMS.Adapters.NoSQL.Configuration;

public class AlertConfiguration : IEntityTypeConfiguration<Alert>
{
    public void Configure(EntityTypeBuilder<Alert> builder)
    {
        // Map to Cosmos container
        builder.ToContainer("Alerts");

        // Partition key (CustomerId is good for queries by customer)
        builder.HasPartitionKey(x => x.CustomerId);

        // Primary key
        builder.HasKey(x => x.Id);

        // Property mappings
        builder.Property(x => x.Id)
            .ToJsonProperty("id");

        builder.Property(x => x.AlertId)
            .ToJsonProperty("alert_id");

        builder.Property(x => x.CustomerId)
            .ToJsonProperty("customer_id");

        builder.Property(x => x.MachineId)
            .ToJsonProperty("machine_id");

        builder.Property(x => x.RuleName)
            .ToJsonProperty("rule_name");

        builder.Property(x => x.Status)
            .ToJsonProperty("status");

        builder.Property(x => x.Message)
            .ToJsonProperty("message");

        builder.Property(x => x.OperationalData)
            .ToJsonProperty("operational_data");

        builder.Property(x => x.TriggeredAt)
            .ToJsonProperty("triggered_at");

        builder.Property(x => x.CreatedAt)
            .ToJsonProperty("created_at");
    }
}
