namespace MMS.Adapters.NoSQL.Configuration;

public class OperationalDataConfiguration : IEntityTypeConfiguration<OperationalData>
{
    public void Configure(EntityTypeBuilder<OperationalData> builder)
    {
        builder.ToContainer("OperationalData");

        builder.HasKey(x => x.Id);

        builder.HasPartitionKey(x => x.CustomerId);

        builder.HasNoDiscriminator();

        builder.Property(x => x.Id)
            .ToJsonProperty("id")
            .IsRequired();

        builder.Property(x => x.MachineId)
            .ToJsonProperty("machine_id")
            .IsRequired();

        builder.Property(x => x.CustomerId)
            .ToJsonProperty("customer_id")
            .IsRequired();

        builder.Property(x => x.Timestamp)
            .ToJsonProperty("timestamp")
            .IsRequired();

        builder.Property(x => x.Type)
            .ToJsonProperty("type")
            .IsRequired();

        builder.OwnsOne(x => x.Measurement, measurement =>
        {
            measurement.ToJsonProperty("measurement");
            measurement.Property(m => m.Value).ToJsonProperty("value");
            measurement.Property(m => m.Unit).ToJsonProperty("unit");
        });
    }
}