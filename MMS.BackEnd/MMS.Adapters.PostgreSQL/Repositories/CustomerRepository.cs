namespace MMS.Adapters.PostgreSQL.Repositories;

public class CustomerRepository(
    ApplicationDbContext dbContext,
    ILogger<CustomerRepository> logger) : MMsCrudRepository<Customer>(dbContext, logger), ICustomerRepository
{
    public async Task<bool> ExistsAsync(Guid customerId)
    {
        return await dbContext.Customers.AnyAsync(c => c.Id == customerId);
    }


    public async Task<bool> ExistsEmail(Expression<Func<Customer, bool>> predicate)
    {
        return await dbContext.Customers.AnyAsync(predicate);
    }


    public async Task<bool> ExistsPhoneAsync(string phoneCountryCode, string phoneNumber, Guid? excludeCustomerId = null)
    {
        var query = dbContext.Customers
            .AsNoTracking()
            .Where(c => c.Deleted == null &&
                        EF.Functions.ILike(c.PhoneCountryCode, phoneCountryCode) &&
                        EF.Functions.ILike(c.PhoneNumber, phoneNumber));

        if (excludeCustomerId.HasValue)
        {
            query = query.Where(c => c.Id != excludeCustomerId.Value);
        }

        return await query.AnyAsync();
    }


    public async Task<List<Customer>> GetAllWithMachinesAsync(
        List<Guid>? customerIds = null,
        string? term = null,
        int skip = 0,
        int take = 10)
    {
        var query = dbContext.Customers
             .AsNoTracking()
            .Include(c => c.Machine.Where(m => m.Deleted == null))
            .Where(c => c.Deleted == null);

        if (customerIds != null && customerIds.Any())
        {
            query = query.Where(c => customerIds.Contains(c.Id));
        }

        if (!string.IsNullOrWhiteSpace(term))
        {
            query = query.Where(c => c.Name != null &&
                EF.Functions.ILike(c.Name, $"%{term}%"));
        }

        return await query
            .OrderBy(c => c.Name)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }



    public async Task<Either<RepositoryError, List<Machine>>> GetMachinesByCustomerIdAsync(Guid customerId)
    {
        try
        {
            var machines = await dbContext.Machines.AsNoTracking()
                .Where(m => m.CustomerId == customerId)
                .Where(c => c.Deleted == null)
                .ToListAsync();

            if (machines == null || machines.Count == 0)
            {
                return new Left<RepositoryError, List<Machine>>(
                    new RepositoryError($"No machines found for Customer ID: {customerId}")
                );
            }

            return new Right<RepositoryError, List<Machine>>(machines);
        }
        catch (Exception ex)
        {
            return new Left<RepositoryError, List<Machine>>(new RepositoryError("Database error: " + ex.Message));
        }
    }

    /// <summary>
    /// ✅ OPTIMIZED: Get machines by customer ID with pagination and search
    /// Uses AsNoTracking for read-only queries and optimized filtering
    /// </summary>
    public async Task<Either<RepositoryError, List<Machine>>> GetMachinesByCustomerIdAsync(
    Guid customerId,
    PageParameters pageParameters)
    {
        try
        {
            if (customerId == Guid.Empty)
            {
                return new Left<RepositoryError, List<Machine>>(
                    new RepositoryError("Invalid customerId: Guid cannot be empty.")
                );
            }

            // ✅ OPTIMIZATION: Build query with AsNoTracking for read-only performance
            var query = dbContext.Machines
                .AsNoTracking()
                .Where(m => m.CustomerId == customerId && m.Deleted == null);

            // ✅ OPTIMIZATION: Pre-trim and normalize search term once
            if (!string.IsNullOrWhiteSpace(pageParameters.Term))
            {
                string term = $"%{pageParameters.Term.Trim()}%";
                query = query.Where(m =>
                    EF.Functions.ILike(m.MachineName, term) ||
                    EF.Functions.ILike(m.MachineModel, term));
            }

            // ✅ OPTIMIZATION: Order before pagination for consistent results
            query = query.OrderBy(m => m.MachineName);

            // ✅ OPTIMIZATION: Apply pagination efficiently
            if (pageParameters.Skip.HasValue && pageParameters.Skip.Value > 0)
                query = query.Skip(pageParameters.Skip.Value);

            if (pageParameters.Top.HasValue && pageParameters.Top.Value > 0)
                query = query.Take(pageParameters.Top.Value);

            var machines = await query.ToListAsync();

            return new Right<RepositoryError, List<Machine>>(machines ?? new List<Machine>());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving machines for customer {CustomerId}", customerId);
            return new Left<RepositoryError, List<Machine>>(
                new RepositoryError("Database error: " + ex.Message)
            );
        }
    }

    public async Task<List<CustomerWithMachinesDto>> GetPagedWithMachinesAsync(List<Guid>? customerIds, string? term, int skip, int take)
    {
        var query = dbContext.Customers
            .AsNoTracking()
            .Where(c => c.Deleted == null)
           .Select(c => new CustomerWithMachinesDto
           {
               Id = c.Id,
               Name = c.Name,
               ImageUrl = c.ImageUrls,
               Machines = c.Machine
                   .Where(m => m.Deleted == null)
                   .Select(m => new MachineDto(
                   m.Id,
                   m.MachineName,
                   m.MachineModel,
                   m.Manufacturer,
                   m.SerialNumber,
                   m.Location,
                   m.InstallationDate,
                   m.CommunicationProtocol,
                   m.MachineType,
                   m.CustomerId,
                   m.ImageUrl
               )).ToList()
           });

        if (customerIds != null && customerIds.Any())
        {
            query = query.Where(c => customerIds.Contains(c.Id));
        }

        if (!string.IsNullOrEmpty(term))
        {
            query = query.Where(c => c.Name != null && EF.Functions.ILike(c.Name, $"%{term}%"));
        }

        query = query
            .OrderBy(c => c.Name)
            .Skip(skip)
            .Take(take);

        return await query.ToListAsync();
    }
}