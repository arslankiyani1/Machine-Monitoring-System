namespace MMS.Adapters.NoSQL.Configuration;

public class MachineJobConfiguration : IEntityTypeConfiguration<MachineJob>
{
    public void Configure(EntityTypeBuilder<MachineJob> builder)
    {
        builder.ToContainer(Constant.MachineJob);
        builder.HasPartitionKey(x => x.CustomerId);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ToJsonProperty("id");
        builder.Property(x => x.JobName).ToJsonProperty("job_name");
        builder.Property(x => x.CustomerId).ToJsonProperty("customer_id");
        builder.Property(x => x.MachineIds).ToJsonProperty("machine_ids");
        builder.Property(x => x.MachineNames).ToJsonProperty("machine_names");
        builder.Property(x => x.OperatorId).ToJsonProperty("operator_id");
        builder.Property(x => x.OperatorName).ToJsonProperty("operator_name");
        builder.Property(x => x.Status).ToJsonProperty("status").HasConversion<string>();
        builder.Property(x => x.StartTime).ToJsonProperty("start_time");
        builder.Property(x => x.EndTime).ToJsonProperty("end_time");
        builder.Property(x => x.PartNumber).ToJsonProperty("part_number");
        builder.Property(x => x.ProgramNo).ToJsonProperty("program_no");
        builder.Property(x => x.MainProgram).ToJsonProperty("main_program");
        builder.Property(x => x.Description).ToJsonProperty("description");
        builder.Property(x => x.OrderNo).ToJsonProperty("order_no");
        builder.Property(x => x.OrderDate).ToJsonProperty("order_date");
        builder.Property(x => x.DueDate).ToJsonProperty("due_date");
        builder.Property(x => x.PriorityLevel).ToJsonProperty("priority_level").HasConversion<string>();
        builder.Property(x => x.JobType).ToJsonProperty("job_level").HasConversion<string>();
        builder.Property(x => x.CreatedAt).ToJsonProperty("created_at");
        builder.Property(x => x.UpdatedAt).ToJsonProperty("updated_at");
        builder.Property(x => x.Dependencies).ToJsonProperty("dependencies");
        builder.Property(x => x.Attachments).ToJsonProperty("attachments");

        builder.OwnsMany(x => x.DowntimeEvents, b =>
        {
            b.ToJsonProperty("downtime_events");
            b.Property(de => de.Reason)
             .ToJsonProperty("reason")
             .HasConversion<string>()
             .IsRequired(true);
            b.Property(de => de.StartTime).ToJsonProperty("start_time");
            b.Property(de => de.EndTime).ToJsonProperty("end_time");
            b.Property(de => de.Duration).ToJsonProperty("duration");
        });

        builder.OwnsOne(x => x.Quantities, b =>
        {
            b.ToJsonProperty("quantities");
            b.Property(q => q.Required).ToJsonProperty("required");
            b.Property(q => q.Completed).ToJsonProperty("completed");
            b.Property(q => q.Good).ToJsonProperty("good");
            b.Property(q => q.Bad).ToJsonProperty("bad");
            b.Property(q => q.InProgress).ToJsonProperty("in_progress");
        });

        builder.OwnsOne(x => x.Metrics, b =>
        {
            b.ToJsonProperty("metrics");
            b.Property(m => m.TargetCycleTime).ToJsonProperty("target_cycle_time");
            b.Property(m => m.ScheduledTimeSeconds).ToJsonProperty("scheduled_time_seconds");
        });

        builder.OwnsOne(x => x.Schedule, b =>
        {
            b.ToJsonProperty("schedule");
            b.Property(s => s.PlannedStart).ToJsonProperty("planned_start");
            b.Property(s => s.PlannedEnd).ToJsonProperty("planned_end");
        });

        builder.OwnsOne(x => x.Setup, b =>
        {
            b.ToJsonProperty("setup");
            b.Property(s => s.StartTime).ToJsonProperty("start_time");
            b.Property(s => s.EndTime).ToJsonProperty("end_time");
        });

        builder.HasNoDiscriminator();
    }
}
