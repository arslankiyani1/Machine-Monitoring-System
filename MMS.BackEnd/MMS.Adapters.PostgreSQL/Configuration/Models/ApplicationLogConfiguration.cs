namespace MMS.Adapters.PostgreSQL.Configuration.Models;

public class ApplicationLogConfiguration : TrackableConfiguration<ApplicationLog>
{
    public override void Configure(EntityTypeBuilder<ApplicationLog> builder)
    {
        base.Configure(builder);
        builder.HasKey(sl => sl.Id);

        builder.Property(e => e.ApiRequest)
               .HasColumnType("jsonb");

        builder.Property(e => e.ApiResponse)
               .HasColumnType("jsonb");
    }
}