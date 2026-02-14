namespace MMS.Adapters.PostgreSQL.Configuration.Models;

public class CustomerDashboardConfiguration : TrackableConfiguration<CustomerDashboard>
{
    public override void Configure(EntityTypeBuilder<CustomerDashboard> builder)
    {
        base.Configure(builder);

        builder.HasKey(cd => cd.Id);

        builder.Property(cd => cd.Theme)
               .HasMaxLength(200);

        builder.Property(cd => cd.Name)
               .IsRequired()
               .HasMaxLength(150);

        builder.Property(e => e.Layout)
               .HasColumnType("jsonb");

        builder
               .HasMany(cd => cd.Widget)
               .WithOne(w => w.CustomerDashboard)
               .HasForeignKey(w => w.DashboardId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}