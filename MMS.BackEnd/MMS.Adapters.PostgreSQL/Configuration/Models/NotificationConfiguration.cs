using System.Text.Json;
namespace MMS.Adapters.PostgreSQL.Configuration.Models;

public class NotificationConfiguration : TrackableConfiguration<Notification>
{
    public override void Configure(EntityTypeBuilder<Notification> builder)
    {
        base.Configure(builder);

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Title)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(n => n.Body)
               .IsRequired();

        // Store List<string> as JSON
        builder.Property(n => n.Recipients)
               .HasColumnType("jsonb")
               .HasConversion(
                   v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                   v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>()
               );

        builder.Property(n => n.MachineId)
               .HasColumnType("uuid")
               .IsRequired(false);

        builder.Property(n => n.MachineName)
               .IsRequired(false);

        builder.Property(n => n.CustomerId)
               .HasColumnType("uuid")
               .IsRequired(false);

        builder.Property(n => n.Priority)
               .IsRequired(false);

        builder.Property(n => n.Link)
               .IsRequired(false);

        builder.Property(n => n.ReadStatus)
               .HasConversion<int>()
               .IsRequired();

        builder.Property(n => n.NotificationTypes)
               .HasConversion<int>()
               .IsRequired();

        builder.Property(n => n.ReadAt)
               .IsRequired(false);

        builder.Property(n => n.UserId)
               .HasColumnType("uuid")
               .IsRequired(false);
    }
}