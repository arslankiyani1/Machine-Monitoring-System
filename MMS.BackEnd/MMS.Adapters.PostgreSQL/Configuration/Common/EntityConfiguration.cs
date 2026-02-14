namespace MMS.Adapters.PostgreSQL.Configuration.Common;

public abstract class EntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
where TEntity : class, IEntity<Guid>
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.Property(c => c.Id).HasDefaultValue(Guid.NewGuid());
    }
}