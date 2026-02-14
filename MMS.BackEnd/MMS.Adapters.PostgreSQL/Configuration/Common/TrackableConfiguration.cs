namespace MMS.Adapters.PostgreSQL.Configuration.Common;

public abstract class TrackableConfiguration<TEntity> : SoftDeleteConfiguration<TEntity>
    where TEntity : Trackable
{
    public override void Configure(EntityTypeBuilder<TEntity> builder)
    {
        base.Configure(builder);
        builder.Property(c => c.CreatedAt)
            .HasField("createdAt")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(c => c.CreatedBy)
            .HasField("createdBy")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(c => c.UpdatedAt)
            .HasField("updatedAt")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(c => c.UpdatedBy)
            .HasField("updatedBy")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}