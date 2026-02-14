using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MMS.Application.Models.SQL;

namespace MMS.Adapters.PostgreSQL.Configuration.Models;

public class MachineSettingConfiguration : TrackableConfiguration<MachineSetting>
{
    public override void Configure(EntityTypeBuilder<MachineSetting> builder)
    {
        base.Configure(builder);

        builder.HasKey(sl => sl.Id);

        // Configure DownTimeReasons as List<string>
        builder.Property(ms => ms.DownTimeReasons)
            .HasConversion(
                // Convert List<string> → string[] for database storage
                v => v.ToArray(),
                // Convert string[] → List<string> when reading from database
                v => v.ToList()
            )
            .Metadata.SetValueComparer(
                new ValueComparer<List<string>>(
                    // Equality comparison
                    (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                    // Hash code generation
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    // Deep copy
                    c => c.ToList()
                )
            );
    }
}