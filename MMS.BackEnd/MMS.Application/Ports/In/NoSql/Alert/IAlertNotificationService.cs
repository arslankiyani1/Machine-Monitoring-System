using MMS.Application.Models.NoSQL.Context;

namespace MMS.Application.Ports.In.NoSql.Alert;

public interface IAlertNotificationService
{
    Task SendAlertAsync(
        AlertContext context,
        AlertRule rule,
        CancellationToken cancellationToken = default);

    Task<bool> EvaluateAndTriggerAlertAsync(
        AlertRule rule,
        Dictionary<string, object> operationalData,
        AlertContext context,
        CancellationToken cancellationToken = default);
}
