namespace MMS.Adapters.PostgreSQL.Configuration.Models;

public class MachineSensorConfiguration : TrackableConfiguration<MachineSensor>
{
    public override void Configure(EntityTypeBuilder<MachineSensor> builder)
    {
        base.Configure(builder);

        builder.ToTable("MachineSensor");

        // Primary Key
        builder.HasKey(ms => ms.Id);

        // Optional FK: MachineId
        builder.Property(ms => ms.MachineId)
               .IsRequired(false);

        // Serial Number - required and unique
        builder.Property(ms => ms.SerialNumber)
               .IsRequired()
               .HasMaxLength(100);

        builder.HasIndex(ms => ms.SerialNumber)
               .IsUnique();

        // Name - required
        builder.Property(ms => ms.Name)
               .IsRequired()
               .HasMaxLength(200);

        // Enums
        builder.Property(ms => ms.Interface)
               .HasConversion<int>() // store enum as text
               .HasMaxLength(50)
               .IsRequired();

        builder.Property(ms => ms.SensorType)
               .HasConversion<int>() // store enum as text
               .HasMaxLength(50)
               .IsRequired();

        // Modbus IP (optional)
        builder.Property(ms => ms.ModbusIp)
               .HasMaxLength(30)
               .IsRequired(false);

        // HRegList — Postgres text array
        builder.Property(ms => ms.HRegList)
               .HasColumnType("text[]")
               .IsRequired();

        // Image URL (optional)
        builder.Property(ms => ms.ImageUrl)
               .HasMaxLength(500)
               .IsRequired(false);
    }

}
