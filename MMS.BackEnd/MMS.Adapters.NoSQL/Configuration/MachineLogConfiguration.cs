using System.Text.Json.Serialization;

public class MachineLogConfiguration : IEntityTypeConfiguration<MachineLog>
{
    public void Configure(EntityTypeBuilder<MachineLog> builder)
    {
        builder.ToContainer(Constant.MachineLog);
        builder.HasPartitionKey(x => x.CustomerId);
        builder.HasKey(x => x.Id);

        // Fix: Explicitly disable discriminator to avoid materialization errors
        builder.HasNoDiscriminator();

        builder.Property(x => x.Id).ToJsonProperty("id");
        builder.Property(x => x.MachineId).ToJsonProperty("machine_id");
        builder.Property(x => x.CustomerId).ToJsonProperty("customer_id");
        builder.Property(x => x.UserId).ToJsonProperty("user_id");
        builder.Property(x => x.JobId).ToJsonProperty("job_id");
        builder.Property(x => x.UserName).ToJsonProperty("user_name");
        builder.Property(x => x.Color).ToJsonProperty("color");
        builder.Property(x => x.Status).ToJsonProperty("status");

        // ✅ JSON options for null ignore
        var jsonOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        // ✅ Converter for Inputs
        var inputsConverter = new ValueConverter<List<MachineInput>, string>(
            v => JsonSerializer.Serialize(v, jsonOptions),
            v => JsonSerializer.Deserialize<List<MachineInput>>(v, jsonOptions) ?? new()
        );

        builder.Property(x => x.Inputs)
            .HasConversion(inputsConverter!)
            .ToJsonProperty("inputs");

        builder.Property(x => x.Start).ToJsonProperty("start");
        builder.Property(x => x.End).ToJsonProperty("end");
        builder.Property(x => x.LastUpdateTime).ToJsonProperty("last_update_time");
        builder.Property(x => x.Comment).ToJsonProperty("comment");
        builder.Property(x => x.MainProgram).ToJsonProperty("main_program");
        builder.Property(x => x.CurrentProgram).ToJsonProperty("current_program");
        builder.Property(x => x.InterfaceName).ToJsonProperty("interface");

        builder.HasNoDiscriminator();
    }
}
