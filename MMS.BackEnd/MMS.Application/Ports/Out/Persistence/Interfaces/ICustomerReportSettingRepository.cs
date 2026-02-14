namespace MMS.Application.Ports.Out.Persistence.Interfaces;

public interface ICustomerReportSettingRepository
{
    Task<List<CustomerReportSetting>> GetReportSettingRepositoryAsync(
        PageParameters pageParameters,
        Expression<Func<CustomerReportSetting, bool>> pageFilterExpression,
        List<Expression<Func<CustomerReportSetting, bool>>>? documentFilterExpression = null,
        Func<IQueryable<CustomerReportSetting>, IOrderedQueryable<CustomerReportSetting>>? order = null
    );

    Task<Either<RepositoryError, CustomerReportSetting>> GetAsync(Guid reportId);
    Task<Either<RepositoryError, CustomerReportSetting>> AddAsync(CustomerReportSetting customerReport);
    Task<Either<RepositoryError, CustomerReportSetting>> UpdateAsync(Guid reportId, CustomerReportSetting customerReport);
    Task<Either<RepositoryError, CustomerReportSetting>> DeleteAsync(Guid reportId);
    Task<bool> ExistsIgnoreCaseAsync(Expression<Func<CustomerReportSetting, string>> propertySelector, string value);
    Task<bool> ExistsAsync(Guid id);
    Task<bool> ExistsEmail(Expression<Func<CustomerReportSetting, bool>> predicate);
    Task<List<CustomerReportSetting>> GetActiveScheduledSettingsAsync();
}