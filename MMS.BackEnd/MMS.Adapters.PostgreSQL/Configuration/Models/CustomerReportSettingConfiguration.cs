using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MMS.Adapters.PostgreSQL.Configuration.Models;

public class CustomerReportSettingConfiguration : TrackableConfiguration<CustomerReportSetting>
{
    public override void Configure(EntityTypeBuilder<CustomerReportSetting> builder)
    {
        base.Configure(builder);

        builder.HasKey(crs => crs.Id);

        // Properties
        builder.Property(crs => crs.ReportName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(crs => crs.Email)
          .HasColumnType("text[]")
          //.IsRequired()
          .HasConversion(
              new ValueConverter<List<string>, string[]>(
                  v => v.ToArray(),
                  v => v.ToList()),
              new ValueComparer<List<string>>(
                  (c1, c2) =>
                      (c1 ?? new List<string>()).SequenceEqual(c2 ?? new List<string>()),
                  c =>
                      (c ?? new List<string>())
                          .Aggregate(0, (a, v) => HashCode.Combine(a, v == null ? 0 : v.GetHashCode())),
                  c => c == null ? null! : c.ToList()
              ));


        builder.Property(crs => crs.Format)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(crs => crs.Frequency)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(crs => crs.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(crs => crs.IsCustomReport)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(crs => crs.WeekDays)
         .HasColumnType("text[]")
         //.IsRequired()
         .HasConversion(
             new ValueConverter<Days[], string[]>(
                 v => v.Select(e => e.ToString()).ToArray(),
                 v => v.Select(e => Enum.Parse<Days>(e)).ToArray()),
             new ValueComparer<Days[]>(
                 (c1, c2) => c1.SequenceEqual(c2),
                 c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                 c => c.ToArray()));

        builder.Property(crs => crs.ReportPeriodStartDate)
            .IsRequired()
            .HasColumnType("timestamp with time zone");

        builder.Property(crs => crs.ReportPeriodEndDate)
            .IsRequired()
            .HasColumnType("timestamp with time zone");

   

        builder.Property(crs => crs.MachineIds)
            .HasColumnType("text[]")
            .IsRequired()
            .HasConversion(
                new ValueConverter<Guid[], string[]>(
                    v => v.Select(g => g.ToString()).ToArray(),
                    v => v.Select(g => Guid.Parse(g)).ToArray()),
                new ValueComparer<Guid[]>(
                    (c1, c2) => c1.SequenceEqual(c2),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToArray()));

                builder.Property(crs => crs.ReportType)
                  .HasColumnType("text[]")
                  .IsRequired()
                  .HasConversion(
                      new ValueConverter<ReportType[], string[]>(
                          v => v.Select(e => e.ToString()).ToArray(),
                          v => v.Select(e => Enum.Parse<ReportType>(e)).ToArray()),
                      new ValueComparer<ReportType[]>(
                          (c1, c2) => c1.SequenceEqual(c2),
                          c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                          c => c.ToArray()));

                builder.Property(crs => crs.CustomerId)
                    .IsRequired();

                // Relationships
                builder.HasOne(crs => crs.Customer)
                .WithMany(c => c.CustomerReportSettings)
                .HasForeignKey(crs => crs.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

    }
}
