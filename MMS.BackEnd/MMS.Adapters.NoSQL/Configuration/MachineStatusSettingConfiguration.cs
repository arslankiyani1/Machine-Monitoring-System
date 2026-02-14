public class MachineStatusSettingConfiguration : IEntityTypeConfiguration<MachineStatusSetting>
{
    public void Configure(EntityTypeBuilder<MachineStatusSetting> builder)
    {
        builder.ToContainer("MachineStatusSetting");
        builder.HasPartitionKey(e => e.MachineId);
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).ToJsonProperty("id");
        builder.Property(e => e.MachineId).ToJsonProperty("machine_id");

        // ✅ Store as native JSON array in Cosmos
        builder.OwnsMany(e => e.Inputs, inputBuilder =>
        {
            inputBuilder.ToJsonProperty("inputs");

            inputBuilder.Property(i => i.InputKey).ToJsonProperty("input_key");
            inputBuilder.Property(i => i.Signals).ToJsonProperty("signals");
            inputBuilder.Property(i => i.Color).ToJsonProperty("color");
            inputBuilder.Property(i => i.Status).ToJsonProperty("status");
        });

        builder.HasNoDiscriminator();
    }
}
