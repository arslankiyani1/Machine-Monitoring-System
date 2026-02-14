namespace MMS.Adapters.PostgreSQL.Configuration.Models;

public class UserMachineConfiguration : TrackableConfiguration<UserMachine>
{
    public override void Configure(EntityTypeBuilder<UserMachine> builder)
    {
        base.Configure(builder);

        builder.HasKey(um => um.Id);

        builder.HasIndex(um => new { um.UserId, um.MachineId })
               .IsUnique();
    }
}