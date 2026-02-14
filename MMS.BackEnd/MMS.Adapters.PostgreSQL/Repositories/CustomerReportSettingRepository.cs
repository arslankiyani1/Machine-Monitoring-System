namespace MMS.Adapters.PostgreSQL.Repositories;

public class CustomerReportSettingRepository(ApplicationDbContext dbContext, ILogger<CustomerReportSettingRepository> logger)
    : MMsCrudRepository<CustomerReportSetting>(dbContext, logger), ICustomerReportSettingRepository
{
    public async Task<bool> ExistsAsync(Guid id)
    {
        return await dbContext.CustomerReportSetting.AnyAsync(x => x.Id == id);
    }

    public async Task<bool> ExistsEmail(Expression<Func<CustomerReportSetting, bool>> predicate)
    {
        return await dbContext.CustomerReportSetting.AnyAsync(predicate);
    }

    public async Task<List<CustomerReportSetting>> GetActiveScheduledSettingsAsync()
    {
        return await dbContext.CustomerReportSetting
            .Where(s => !s.IsCustomReport && s.Frequency != ReportFrequency.None && s.Deleted == null)
            .ToListAsync();
    }

    public async Task<List<CustomerReportSetting>> GetReportSettingRepositoryAsync(
       PageParameters pageParameters,
       Expression<Func<CustomerReportSetting, bool>> pageFilterExpression,
       List<Expression<Func<CustomerReportSetting, bool>>>? documentFilterExpression = null,
       Func<IQueryable<CustomerReportSetting>, IOrderedQueryable<CustomerReportSetting>>? order = null
     )
    {
        // Start with base query including CustomerReports
        var query = dbContext.CustomerReportSetting
            .Include(s => s.CustomerReports)
            .Where(s => ((ISoftDelete)s).Deleted == null)
            .AsQueryable();

        // Apply search expression
        query = query.Where(pageFilterExpression);

        // Apply additional filters
        if (documentFilterExpression != null)
        {
            foreach (var filter in documentFilterExpression)
            {
                query = query.Where(filter);
            }
        }

        // Apply ordering if provided
        if (order != null)
        {
            query = order(query);
        }

        // Apply pagination
        query = query.Skip(pageParameters.Skip ?? 0)
                     .Take(pageParameters.Top ?? 10);

        return await query.ToListAsync();
    }
}