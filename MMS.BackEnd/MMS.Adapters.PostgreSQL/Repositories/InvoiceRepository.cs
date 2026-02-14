namespace MMS.Adapters.PostgreSQL.Repositories;

public class InvoiceRepository(ApplicationDbContext dbContext, ILogger<InvoiceRepository> logger)
    : MMsCrudRepository<Invoice>(dbContext, logger), IInvoiceRepository
{}
