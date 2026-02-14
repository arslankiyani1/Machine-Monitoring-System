using System.Net.Mail;
using System.Text;

namespace MMS.Application.Services.NoSql;

public class AlertNotificationService(
     IAlertRepository _alertRepository,
     IAlertRuleRepository _alertRuleRepository,
     IEmailService _emailService,
     INotificationService _notificationService,
     ISmsNotificationService _smsService,
     IAlertEvaluationService _alertEvaluation) : IAlertNotificationService
{
    public async Task<bool> EvaluateAndTriggerAlertAsync(
        AlertRule rule,
        Dictionary<string, object> operationalData,
        AlertContext context,
        CancellationToken cancellationToken = default)
    {
        if (rule == null || !rule.Enabled)
            return false;

        bool triggered = _alertEvaluation.Evaluate(rule, operationalData);
        if (!triggered)
            return false;

        rule.LastTriggered = DateTime.UtcNow;
        await _alertRuleRepository.UpdateAsync(rule, cancellationToken);

        await SendAlertAsync(context, rule, cancellationToken);
        return true;
    }

    public async Task SendAlertAsync(
        AlertContext context,
        AlertRule rule,
        CancellationToken cancellationToken = default)
    {
        if (context == null || rule == null)
            return;

        if (rule.AlertActions == null || !rule.AlertActions.Any())
        {
            Console.WriteLine($"⚠️ No alert actions configured for rule: {rule.RuleName}");
            return;
        }

        // Save alert record
        await SaveAlertRecordAsync(context, rule, cancellationToken);

        var alertTime = (rule.LastTriggered ?? DateTime.UtcNow).ToString("yyyy-MM-dd HH:mm:ss UTC");

        // Process each alert action
        foreach (var action in rule.AlertActions)
        {
            if (action == null) continue;

            if (action.Type == Types.email)
            {
                var alertMessage = BuildAlertMessage(action.Message, context, rule.RuleName);
                await SendEmailAlertsAsync(
                    action.Recipients,
                    context.MachineName,
                    alertMessage,
                    alertTime,
                    context.MachineId,
                    rule.RuleName);
            }
            else if (action.Type == Types.push)
            {
                await SendPushAlertsAsync(
                    action.Recipients,
                    rule,
                    context,
                    cancellationToken);
            }
            else if (action.Type == Types.sms)
            {
                var alertMessage = BuildSmsAlertMessage(action.Message, context, rule.RuleName, alertTime);
                await SendSmsAlertsAsync(
                    action.Recipients,
                    context.MachineName,
                    alertMessage,
                    context.MachineId,
                    rule.RuleName);
            }
        }
    }

    private async Task SaveAlertRecordAsync(
        AlertContext context,
        AlertRule rule,
        CancellationToken cancellationToken)
    {
        var alert = new MMS.Application.Models.NoSQL.Alert
        {
            CustomerId = context.CustomerId,
            MachineId = context.MachineId,
            RuleName = rule.RuleName,
            Status = context.Status,
            Message = string.Join("\n", rule.AlertActions?.Select(a => a.Message ?? "") ?? Enumerable.Empty<string>()),
            OperationalData = context.OperationalData,
            TriggeredAt = rule.LastTriggered ?? DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            await _alertRepository.AddAsync(alert, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error saving alert record for MachineId: {context.MachineId}, Rule: {rule.RuleName}. Exception: {ex.Message}");
        }
    }

    private static string BuildAlertMessage(string? actionMessage, AlertContext context, string ruleName)
    {
        var sb = new StringBuilder();
        sb.AppendLine(actionMessage ?? "Alert triggered");
        sb.AppendLine();
        sb.AppendLine($"Alert Rule: {ruleName}");
        sb.AppendLine($"Machine: {context.MachineName}");
        sb.AppendLine($"Status: {context.Status}");

        if (!string.IsNullOrWhiteSpace(context.Source))
            sb.AppendLine($"Source: {context.Source}");

        if (!string.IsNullOrWhiteSpace(context.JobId))
            sb.AppendLine($"Job ID: {context.JobId}");

        if (!string.IsNullOrWhiteSpace(context.UserName) && context.UserName != "N/A")
            sb.AppendLine($"Operator: {context.UserName}");

        sb.AppendLine($"Details: {context.OperationalData}");
        sb.AppendLine($"Time: {context.TriggeredAt:yyyy-MM-dd HH:mm:ss UTC}");

        return sb.ToString();
    }

    private static string BuildPushNotificationBody(AlertContext context)
    {
        // Determine the alert type from Status and OperationalData
        var status = context.Status?.ToLower() ?? "";
        var operationalData = context.OperationalData?.ToLower() ?? "";

        string alertType = "Alert";

        // Determine specific alert type based on status/operational data
        if (status.Contains("cycle") || operationalData.Contains("cycle"))
        {
            alertType = "Cycle Stop";
        }
        else if (status.Contains("downtime") || operationalData.Contains("downtime"))
        {
            alertType = "Downtime";
        }
        else if (status.Contains("offline") || operationalData.Contains("offline") || status.Contains("stopped"))
        {
            alertType = "Offline Status";
        }
        else if (status.Contains("maintenance") || operationalData.Contains("maintenance"))
        {
            alertType = "Maintenance Required";
        }
        else if (status.Contains("error") || operationalData.Contains("error"))
        {
            alertType = "Error Detected";
        }
        else if (status.Contains("warning") || operationalData.Contains("warning"))
        {
            alertType = "Warning";
        }
        else if (operationalData.Contains("sensor") || status.Contains("sensor"))
        {
            alertType = "Sensor Issue";
        }
        else if (operationalData.Contains("job") || status.Contains("job"))
        {
            alertType = "Job Issue";
        }
        else if (!string.IsNullOrEmpty(context.Status))
        {
            // Use the status as the alert type if it doesn't match known patterns
            alertType = context.Status;
        }

        return $"{alertType} has been detected on machine {context.MachineName}";
    }

    private static string BuildSmsAlertMessage(string? actionMessage, AlertContext context, string ruleName, string alertTime)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"ALERT: {ruleName}");
        sb.AppendLine($"Machine: {context.MachineName}");

        if (!string.IsNullOrWhiteSpace(actionMessage))
        {
            sb.AppendLine($"Message: {actionMessage}");
        }

        sb.AppendLine($"Status: {context.Status}");

        if (!string.IsNullOrWhiteSpace(context.OperationalData))
        {
            sb.AppendLine($"Details: {context.OperationalData}");
        }

        sb.AppendLine($"Time: {alertTime}");

        // SMS messages should be concise, so limit to 160 characters if needed
        var message = sb.ToString();
        return message.Length > 160 ? message.Substring(0, 157) + "..." : message;
    }

    private async Task SendEmailAlertsAsync(
        IEnumerable<string>? recipients,
        string machineName,
        string alertMessage,
        string alertTime,
        Guid machineId,
        string ruleName)
    {
        if (recipients == null || !recipients.Any())
        {
            Console.WriteLine($"⚠️ No email recipients configured for rule: {ruleName}");
            return;
        }

        foreach (var recipient in recipients)
        {
            if (string.IsNullOrWhiteSpace(recipient) || !IsValidEmail(recipient))
            {
                continue;
            }

            try
            {
                await _emailService.SendMachineAlertEmailAsync(recipient, machineName, alertMessage, alertTime);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error sending email to {recipient} for MachineId: {machineId}, Rule: {ruleName}. Exception: {ex.Message}");
            }
        }
    }

    private string DetermineAlertLink(AlertRule rule, AlertContext context)
    {
        var scope = rule.AlertScope;

        return scope switch
        {
            AlertScope.Machine => "/customer-details",
            AlertScope.Operational => "/customer-machine",
            AlertScope.Sensor => "/sensor-details",
            _ => "/customer-details" // Default fallback
        };

    }

    private string DetermineAlertTitle(AlertRule rule, AlertContext context)
    {
        var scope = rule.AlertScope.ToString() ?? "";

        if (scope.Contains("Machine"))
            return "Machine Alert";
        else if (scope.Contains("Operational"))
            return "Operational Alert";
        else if (scope.Contains("Sensor"))
            return "Sensor Alert";

        return "Machine Alert";
    }

    private async Task SendPushAlertsAsync(
       IEnumerable<string>? recipients,
       AlertRule rule,
       AlertContext context,
       CancellationToken cancellationToken)
    {
        if (recipients == null || !recipients.Any())
        {
            return;
        }

        try
        {
            var alertLink = DetermineAlertLink(rule, context);
            var alertTitle = DetermineAlertTitle(rule, context);

            var request = new AddNotificationDto(
                Title: alertTitle,
                Body: BuildPushNotificationBody(context),
                Recipients: recipients.ToList(),
                MachineId: context.MachineId,
                CustomerId: context.CustomerId,
                Priority: context.Priority,
                Link: alertLink,
                MachineName: context.MachineName,
                NotificationTypes: NotificationTypes.Alert,
                UserIds: null!
            );

            await _notificationService.AddAlertAsync(request, cancellationToken);
            Console.WriteLine($"✅ Push notification sent for Machine: {context.MachineName}, Rule: {rule.RuleName}, Title: {alertTitle}, Link: {alertLink}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error sending push notifications for MachineId: {context.MachineId}, Rule: {rule.RuleName}. Exception: {ex.Message}");
        }
    }

    private async Task SendSmsAlertsAsync(
        IEnumerable<string>? recipients,
        string machineName,
        string alertMessage,
        Guid machineId,
        string ruleName)
    {
        if (recipients == null || !recipients.Any())
        {
            Console.WriteLine($"⚠️ No SMS recipients configured for rule: {ruleName}");
            return;
        }

        foreach (var recipient in recipients)
        {
            if (string.IsNullOrWhiteSpace(recipient))
            {
                continue;
            }

            try
            {
                var smsDto = new MMS.Application.Ports.In.TwilioSms.Dto.SendSmsDto
                {
                    To = recipient,
                    Message = alertMessage
                };

                var result = await _smsService.SendCustomSmsAsync(smsDto);
                if (result.StatusCode == 200)
                {
                    Console.WriteLine($"✅ SMS sent to {MaskPhoneNumber(recipient)} for Machine: {machineName}, Rule: {ruleName}");
                }
                else
                {
                    Console.WriteLine($"❌ Failed to send SMS to {MaskPhoneNumber(recipient)} for MachineId: {machineId}, Rule: {ruleName}. Error: {result.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error sending SMS to {MaskPhoneNumber(recipient)} for MachineId: {machineId}, Rule: {ruleName}. Exception: {ex.Message}");
            }
        }
    }

    private static string MaskPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber) || phoneNumber.Length < 4)
            return "****";

        return $"{phoneNumber[..^4]}****";
    }

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var addr = new MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}