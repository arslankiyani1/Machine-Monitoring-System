namespace MMS.Adapters.NoSQL.Repositories;

public class AlertRepository(MyCosmosDbContext dbContext) : IAlertRepository
{
    public async Task AddAsync(Alert alert, CancellationToken cancellationToken = default)
    {
        await dbContext.Alerts.AddAsync(alert, cancellationToken);
    }
}
