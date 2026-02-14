
namespace MMS.Adapters.PostgreSQL.Configuration.Models;

public class InvoiceConfiguration : TrackableConfiguration<Invoice>
{
    public override void Configure(EntityTypeBuilder<Invoice> builder)
    {
        base.Configure(builder); // Applies CreatedAt, UpdatedAt, etc.

        builder.ToTable("Invoices");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Invoicenumber)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(i => i.Payment)
               .IsRequired();

        builder.Property(i => i.Amout)
               .HasColumnType("decimal(18,2)")
               .IsRequired();

        builder.Property(i => i.Tax)
               .HasColumnType("decimal(18,2)")
               .IsRequired();

        builder.Property(i => i.Status)
               .IsRequired()
               .HasMaxLength(50);

        builder.Property(i => i.Paymentmethod)
               .IsRequired()
               .HasMaxLength(50);

        builder.Property(i => i.PaymentGatewayTrxId)
               .HasMaxLength(100);

        builder.Property(i => i.CustomerSubscriptionId)
               .IsRequired();

        builder.Property(i => i.CustomerId)
               .IsRequired();

        builder.Property(i => i.BillingAdressId)
               .IsRequired();

    }
}
