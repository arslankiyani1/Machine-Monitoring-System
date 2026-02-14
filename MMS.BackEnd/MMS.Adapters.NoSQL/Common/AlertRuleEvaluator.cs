namespace MMS.Adapters.NoSQL.Common;

public class AlertRuleEvaluator : IAlertEvaluationService
{
    private const double Tolerance = 0.0001;

    public bool Evaluate(AlertRule rule, Dictionary<string, object> operationalData)
    {
        if (rule == null || !rule.Enabled || rule.Conditions == null || rule.Conditions.Count == 0)
            return false;

        if (operationalData == null || operationalData.Count == 0)
            return false;

        var conditionResults = new List<bool>();

        foreach (var condition in rule.Conditions)
        {
            bool result = false;

            var key = condition.Parameter.ToString().ToLower();
            var dictKey = operationalData.Keys.FirstOrDefault(k => k.ToLower() == key);

            if (dictKey != null && operationalData.TryGetValue(dictKey, out var rawValue))
            {
                if (rawValue is IConvertible && condition.Threshold is IConvertible)
                {
                    double currentValue = Convert.ToDouble(rawValue);
                    double threshold = Convert.ToDouble(condition.Threshold);

                    result = condition.ConditionType switch
                    {
                        ConditionTypes.gt => currentValue > threshold,
                        ConditionTypes.gte => currentValue >= threshold,
                        ConditionTypes.lt => currentValue < threshold,
                        ConditionTypes.lte => currentValue <= threshold,
                        ConditionTypes.eq => Math.Abs(currentValue - threshold) < Tolerance,
                        _ => false
                    };
                }
            }

            conditionResults.Add(result);
        }

        return rule.Logic switch
        {
            Logic.AND => conditionResults.All(x => x),
            Logic.OR => conditionResults.Any(x => x),
            _ => false
        };
    }
}

