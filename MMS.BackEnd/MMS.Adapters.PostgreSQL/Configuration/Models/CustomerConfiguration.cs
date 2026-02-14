namespace MMS.Adapters.PostgreSQL.Configuration.Models;

public class CustomerConfiguration : TrackableConfiguration<Customer>
{
    public override void Configure(EntityTypeBuilder<Customer> builder)
    {
        base.Configure(builder);

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
               .IsRequired()
               .HasMaxLength(150);

        builder.Property(c => c.Email)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(c => c.PhoneNumber)
               .HasMaxLength(25);

        builder.Property(e => e.Shifts)
               .HasColumnType("jsonb");

        builder
               .HasOne(c => c.CustomerDashboard)
               .WithOne(cd => cd.Customer)
               .HasForeignKey<CustomerDashboard>(cd => cd.CustomerId)
               .OnDelete(DeleteBehavior.Cascade);

        builder
               .HasMany(c => c.Machine)
               .WithOne(m => m.Customer)
               .HasForeignKey(m => m.CustomerId)
               .OnDelete(DeleteBehavior.Cascade);

        builder 
               .HasMany(c => c.CustomerReportSettings)
               .WithOne(cr => cr.Customer)
               .HasForeignKey(cr => cr.CustomerId)
               .OnDelete(DeleteBehavior.Cascade);


        // ✅ Index on Id (works as CustomerId in child tables)
        builder.HasIndex(c => c.Id)
               .HasDatabaseName("IX_Customer_CustomerId");

        builder.HasIndex(c => c.Name)
               .HasDatabaseName("IX_Customer_Name")
               .HasMethod("btree")
               .HasOperators("varchar_pattern_ops"); // For ILike searches

        builder.HasIndex(c => c.Deleted)
               .HasDatabaseName("IX_Customer_Deleted");

        builder.HasIndex(c => c.Email)
               .HasDatabaseName("IX_Customer_Email")
               .HasMethod("btree")
               .HasOperators("varchar_pattern_ops"); // For case-insensitive email checks
    }
}