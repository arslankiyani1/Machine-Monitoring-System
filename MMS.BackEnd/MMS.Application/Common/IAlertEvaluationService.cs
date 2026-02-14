namespace MMS.Application.Common;

public interface IAlertEvaluationService
{
    bool Evaluate(AlertRule rule, Dictionary<string, object> operationalData);
}
