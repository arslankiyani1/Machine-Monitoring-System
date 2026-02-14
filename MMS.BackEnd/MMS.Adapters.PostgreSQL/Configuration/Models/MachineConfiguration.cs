namespace MMS.Adapters.PostgreSQL.Configuration.Models;

public class MachineConfiguration : TrackableConfiguration<Machine>
{
    public override void Configure(EntityTypeBuilder<Machine> builder)
    {
        base.Configure(builder);

        builder.HasKey(m =>m.Id);

        builder.Property(m => m.MachineName)
               .IsRequired()
               .HasMaxLength(150);

        builder.Property(m => m.MachineModel)
               .IsRequired(false)
               .HasMaxLength(100);

        builder.Property(m => m.SerialNumber)
               .IsRequired(false)
               .HasMaxLength(100);

        builder
               .HasOne(m => m.MachineSetting)
               .WithOne(ms => ms.Machine)
               .HasForeignKey<MachineSetting>(ms => ms.MachineId)
               .OnDelete(DeleteBehavior.Cascade);

        builder
               .HasMany(m => m.UserMachine)
               .WithOne(um => um.Machine)
               .HasForeignKey(um => um.MachineId)
               .OnDelete(DeleteBehavior.Cascade);

        builder
               .HasMany(m => m.MachineMaintenanceTask)
               .WithOne(mmt => mmt.Machine)
               .HasForeignKey(mmt => mmt.MachineId)
               .OnDelete(DeleteBehavior.Cascade);

        // ✅ Only keep index on CustomerId
        builder.HasIndex(m => m.CustomerId)
               .HasDatabaseName("idx_machine_customerid");

        // Required Indexes
        builder.HasIndex(m => m.CustomerId)
               .HasDatabaseName("IX_Machine_CustomerId");

        builder.HasIndex(m => new { m.CustomerId, m.Deleted, m.MachineName })
               .HasDatabaseName("IX_Machine_CustomerId_Deleted_MachineName");

        // Conditional Index (include if MachineModel searches are frequent)
        builder.HasIndex(m => m.MachineModel)
               .HasDatabaseName("IX_Machine_MachineModel")
               .HasMethod("btree")
               .HasOperators("varchar_pattern_ops"); // For ILike searches

        // Optional Index (include if SerialNumber is commonly queried in ExistsAsync)
        builder.HasIndex(m => m.SerialNumber)
               .HasDatabaseName("IX_Machine_SerialNumber")
               .HasMethod("btree")
               .HasOperators("varchar_pattern_ops"); // For case-insensitive searches, if needed
    }
}