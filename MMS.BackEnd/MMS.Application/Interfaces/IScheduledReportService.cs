namespace MMS.Application.Interfaces;

/// <summary>
/// Service interface for scheduled report generation
/// </summary>
public interface IScheduledReportService
{
    /// <summary>
    /// Retrieves all CustomerReportSettings that are due for scheduled execution on the given date.
    /// </summary>
    Task<IEnumerable<CustomerReportSetting>> GetDueReportSettingsAsync(DateTime runDate);

    /// <summary>
    /// Generates and sends a report for the given CustomerReportSetting.
    /// Returns true if successful, false otherwise.
    /// </summary>
    Task<bool> GenerateAndSendReportAsync(CustomerReportSetting setting, DateTime runDate);
}
