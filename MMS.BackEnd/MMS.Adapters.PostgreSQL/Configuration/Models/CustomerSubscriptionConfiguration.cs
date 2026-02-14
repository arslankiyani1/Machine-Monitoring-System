namespace MMS.Adapters.PostgreSQL.Configuration.Models;

public class CustomerSubscriptionConfiguration : TrackableConfiguration<CustomerSubscription>
{
    public override void Configure(EntityTypeBuilder<CustomerSubscription> builder)
    {
        base.Configure(builder);

        builder.HasKey(cs => cs.Id);

    }
}