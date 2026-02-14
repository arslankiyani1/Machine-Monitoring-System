namespace MMS.Adapters.PostgreSQL.Repositories;

public class WidgetRepository(ApplicationDbContext dbContext,ILogger<WidgetRepository> logger) 
    : MMsCrudRepository<Widget>(dbContext, logger), IWidgetRepository
{}