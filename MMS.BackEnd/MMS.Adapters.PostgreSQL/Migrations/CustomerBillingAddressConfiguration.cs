namespace MMS.Adapters.PostgreSQL.Configuration.Models;

public class CustomerBillingAddressConfiguration : TrackableConfiguration<CustomerBillingAddress>
{
    public override void Configure(EntityTypeBuilder<CustomerBillingAddress> builder)
    {
        base.Configure(builder); // Applies CreatedAt, UpdatedAt, etc.

        builder.ToTable("CustomerBillingAddresses");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Country)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(c => c.Region)
               .HasMaxLength(100);

        builder.Property(c => c.ZipCode)
               .IsRequired()
               .HasMaxLength(20);

        builder.Property(c => c.City)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(c => c.State)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(c => c.street)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(c => c.CustomerId)
               .IsRequired();
    }
}
