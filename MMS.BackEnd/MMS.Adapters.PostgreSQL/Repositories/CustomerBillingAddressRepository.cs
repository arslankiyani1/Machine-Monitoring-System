namespace MMS.Adapters.PostgreSQL.Repositories;

public class CustomerBillingAddressRepository(
    ApplicationDbContext dbContext,
    ILogger<CustomerBillingAddressRepository> logger)
    : MMsCrudRepository<CustomerBillingAddress>(dbContext, logger),
      ICustomerBillingAddressRepository
{}
