namespace MMS.Adapters.PostgreSQL.Configuration.Models;

public class WidgetConfiguration : TrackableConfiguration<Widget>
{
    public override void Configure(EntityTypeBuilder<Widget> builder)
    {
        base.Configure(builder);

        builder.Property(e => e.Config)
               .HasColumnType("jsonb");
    }
}