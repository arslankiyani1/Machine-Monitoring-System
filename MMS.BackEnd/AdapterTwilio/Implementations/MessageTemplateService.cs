using AdapterTwilio.Abstractions;

namespace AdapterTwilio.Implementations;

/// <summary>
/// Service for processing message templates (Single Responsibility Principle)
/// </summary>
public class MessageTemplateService : IMessageTemplateService
{
    public string ProcessOfflineAlertTemplate(string template, string machineName, DateTime offlineSince)
    {
        if (string.IsNullOrWhiteSpace(template))
            throw new ArgumentException("Template cannot be null or empty", nameof(template));

        if (string.IsNullOrWhiteSpace(machineName))
            throw new ArgumentException("Machine name cannot be null or empty", nameof(machineName));

        return template
            .Replace("{MachineName}", machineName)
            .Replace("{Time}", offlineSince.ToString("yyyy-MM-dd HH:mm:ss UTC"));
    }
}
