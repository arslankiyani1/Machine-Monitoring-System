namespace AdapterTwilio.Abstractions;

/// <summary>
/// Interface for message template processing (Single Responsibility Principle)
/// </summary>
public interface IMessageTemplateService
{
    string ProcessOfflineAlertTemplate(string template, string machineName, DateTime offlineSince);
}
