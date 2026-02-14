using MMS.Application.Enum;

namespace MMS.Adapters.PostgreSQL.Configuration.Models
{
    public class MachineMaintenanceTaskConfiguration : TrackableConfiguration<MachineMaintenanceTask>
    {
        public override void Configure(EntityTypeBuilder<MachineMaintenanceTask> builder)
        {
            base.Configure(builder);

            // ✅ Primary Key
            builder.HasKey(t => t.Id);

            // ✅ Properties
            builder.Property(t => t.MaintenanceTaskName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(t => t.Reason)
                .HasMaxLength(500);

            builder.Property(t => t.Notes)
                .HasMaxLength(2000);

            builder.Property(t => t.IsFinished)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(t => t.Attachments)
                .HasColumnType("text[]")
                .IsRequired();

            builder.Property(t => t.Category)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(t => t.Priority)
                .IsRequired()
                .HasConversion<string>()
                .HasDefaultValue(PriorityLevel.Medium);

            builder.Property(t => t.AssignedToUserId)
                .IsRequired(false);

            builder.Property(t => t.AssignedToUserName)
                .HasMaxLength(100)
                .IsRequired(false);

            builder.Property(t => t.JobId)
                .IsRequired(false);

            builder.Property(t => t.MachineId)
                .IsRequired();

            builder.Property(t => t.CustomerId)
                .IsRequired();

            builder.Property(t => t.StartTime)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            //builder.Property(t => t.EndTime)
            //    .IsRequired(false);

            //builder.Property(t => t.ScheduledDate)
            //    .IsRequired(false);

            //builder.Property(t => t.DueDate)
            //    .IsRequired(false);

            // ✅ Indexes for performance
            builder.HasIndex(t => t.MachineId);
            builder.HasIndex(t => t.CustomerId);
            builder.HasIndex(t => t.ScheduledDate);
            builder.HasIndex(t => t.DueDate);
        }
    }
}
