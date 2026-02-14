namespace MMS.Adapters.PostgreSQL.Configuration.Common;

public abstract class SoftDeleteConfiguration<TEntity> : EntityConfiguration<TEntity>
where TEntity : Base
{
    public override void Configure(EntityTypeBuilder<TEntity> builder)
    {
        base.Configure(builder);
        builder.Property(c => c.Deleted)
            .HasField("deleted")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(c => c.DeletedBy)
            .HasField("deletedBy")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}