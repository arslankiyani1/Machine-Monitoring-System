namespace MMS.Adapters.PostgreSQL.Repositories;

public class UserMachineRepository(
    ApplicationDbContext dbContext,
    ILogger<UserMachineRepository> logger) : MMsCrudRepository<UserMachine>(dbContext, logger), IUserMachineRepository
{
    public async Task<bool> ExistsAsync(Expression<Func<UserMachine, bool>> predicate)
    {
        return await dbContext.UserMachines.AnyAsync(predicate);
    }

    public async Task DeleteRangeAsync(List<UserMachine> userMachines)
    {
        dbContext.UserMachines.RemoveRange(userMachines);
        await dbContext.SaveChangesAsync();
    }

}