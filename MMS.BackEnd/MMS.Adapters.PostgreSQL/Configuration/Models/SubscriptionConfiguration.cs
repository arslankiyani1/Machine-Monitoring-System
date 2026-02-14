namespace MMS.Adapters.PostgreSQL.Configuration.Models;

public class SubscriptionConfiguration : TrackableConfiguration<Subscription>
{
    public override void Configure(EntityTypeBuilder<Subscription> builder)
    {
        base.Configure(builder);

        builder.HasKey(s => s.Id);

        builder.Property(s => s.BillingCycle)
               .IsRequired()
               .HasMaxLength(50);

        builder.Property(s => s.Features)
               .HasColumnType("jsonb");
    }
}