namespace MMS.Adapters.NoSQL.Configuration
{
    public class HistoricalStatsConfiguration : IEntityTypeConfiguration<HistoricalStats>
    {
        public void Configure(EntityTypeBuilder<HistoricalStats> builder)
        {
            builder.ToContainer(Constant.HistoricalStats);
            builder.HasPartitionKey(x => x.MachineId);
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id).ToJsonProperty("id");
            builder.Property(x => x.MachineId).ToJsonProperty("machine_id");
            builder.Property(x => x.CustomerId).ToJsonProperty("customer_id");
            builder.Property(x => x.GeneratedDate).ToJsonProperty("generated_date");
            builder.Property(x => x.DownTime).ToJsonProperty("down_time");
            builder.Property(x => x.OEE).ToJsonProperty("oee");
            builder.Property(x => x.Availability).ToJsonProperty("availability");
            builder.Property(x => x.Performance).ToJsonProperty("performance");
            builder.Property(x => x.Quality).ToJsonProperty("quality");
            builder.Property(x => x.Utilization).ToJsonProperty("utilization");
            builder.Property(x => x.QtyCompleted).ToJsonProperty("qty_completed");
            builder.Property(x => x.QtyGood).ToJsonProperty("qty_good");
            builder.Property(x => x.QtyBad).ToJsonProperty("qty_bad");
            builder.Property(x => x.JobIds).ToJsonProperty("job_ids");

            builder.OwnsMany(x => x.HistoricalDowntimeEvents, b =>
            {
                b.ToJsonProperty("historical_downtime_events");
                b.Property(hde => hde.Reason).ToJsonProperty("reason").HasConversion<string>();
                b.Property(hde => hde.TotalDuration).ToJsonProperty("duration");
            });

            builder.OwnsMany(x => x.JobMetrics, b =>
            {
                b.ToJsonProperty("job_metrics");
                b.Property(jm => jm.Id).ToJsonProperty("id");
                b.Property(jm => jm.JobName).ToJsonProperty("job_name");
                b.Property(jm => jm.StartTime).ToJsonProperty("start_time");
                b.Property(jm => jm.EndTime).ToJsonProperty("end_time");
                b.Property(jm => jm.Status).ToJsonProperty("status").HasConversion<string>();
                b.Property(jm => jm.OEE).ToJsonProperty("oee");
                b.Property(jm => jm.ProgramNo).ToJsonProperty("program_no");
                b.Property(jm => jm.OperatorName).ToJsonProperty("operator_name");
                b.Property(jm => jm.Availability).ToJsonProperty("availability");
                b.Property(jm => jm.Performance).ToJsonProperty("performance");
                b.Property(jm => jm.Quality).ToJsonProperty("quality");
                b.Property(jm => jm.QtyCompleted).ToJsonProperty("qty_completed");
                b.Property(jm => jm.QtyGood).ToJsonProperty("qty_good");
                b.Property(jm => jm.QtyBad).ToJsonProperty("qty_bad");
            });

            builder.HasNoDiscriminator();
        }
    }
}
