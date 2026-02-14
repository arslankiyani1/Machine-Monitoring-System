namespace MMS.Adapters.PostgreSQL.Configuration.Models;

public class CustomerReportConfiguration : TrackableConfiguration<CustomerReport>
{
    public void Configure(EntityTypeBuilder<CustomerReport> builder)
    {
        builder.HasKey(cr => cr.Id);

        builder.Property(cr => cr.ReportName)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(cr => cr.BlobLink)
               .IsRequired();

        builder.Property(cr => cr.Format)
               .HasConversion<string>()
               .IsRequired();

        builder.Property(cr => cr.IsSent)
               .IsRequired();

        builder.Property(cr => cr.GeneratedDate)
               .IsRequired();

        builder.HasOne(cr => cr.CustomerReportSetting)
               .WithMany()
               .HasForeignKey(cr => cr.CustomerReportSettingId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cr => cr.Customer)
               .WithMany()
               .HasForeignKey(cr => cr.CustomerId)
               .OnDelete(DeleteBehavior.Cascade);

    }
}
