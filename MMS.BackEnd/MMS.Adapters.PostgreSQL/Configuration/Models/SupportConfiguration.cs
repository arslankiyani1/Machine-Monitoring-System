public class SupportConfiguration : TrackableConfiguration<Support>
{
    public override void Configure(EntityTypeBuilder<Support> builder)
    {
        base.Configure(builder);

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Description)
               .IsRequired()
               .HasColumnType("text");

        builder.Property(s => s.CreatedAt)
               .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(s => s.SupportType)
               .HasConversion<string>()
               .IsRequired()
               .HasColumnType("text"); // Explicitly specify

        builder.Property(s => s.PriorityLevel)
               .HasConversion<string>()
               .IsRequired()
               .HasColumnType("text");

        builder.Property(s => s.Status)
               .HasConversion<string>()
               .IsRequired()
               .HasColumnType("text");

        builder.Property(s => s.MachineSerialNumber)
               .HasMaxLength(100)
               .IsRequired(false);

        builder.Property(s => s.CustomerName)
               .HasMaxLength(200)
               .IsRequired(false);

        builder.Property(s => s.MachineName)
               .HasMaxLength(200)
               .IsRequired(false);

        builder.Property(s => s.Urls)
               .HasColumnType("text")
               .IsRequired(false);


    }
}