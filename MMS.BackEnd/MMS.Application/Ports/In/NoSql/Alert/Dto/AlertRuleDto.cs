namespace MMS.Application.Ports.In.NoSql.Alert.Dto;

public record AlertRuleDto(
    string Id,
    Guid CustomerId,
    Guid? MachineId,                    // Optional
    string RuleName,
    Logic Logic,
    Guid? SensorId,                     // Optional
    AlertScope AlertScope,
    List<ConditionDto> Conditions,
    List<AlertActionDto> AlertActions,
    PriorityLevel Priority,
    bool Enabled,
    DateTime? LastTriggered,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
public record AddAlertRuleDto(
        Guid CustomerId,
        Guid MachineId,
        string RuleName,
        Logic Logic,
        Guid SensorId,
        AlertScope AlertScope,
        List<ConditionDto> Conditions,
        List<AlertActionDto> AlertActions,
        PriorityLevel Priority,
        bool Enabled
);

public record UpdateAlertRuleDto(
    Guid Id,
    Guid CustomerId,
    Guid? MachineId,
    string RuleName,
    Logic Logic,
    Guid? SensorId,
    AlertScope AlertScope,
    List<ConditionDto> Conditions,
    List<AlertActionDto> AlertActions,
    PriorityLevel Priority,
    bool Enabled,
    DateTime? LastTriggered
);

public record ConditionDto(
        ParameterType Parameter,
        ConditionTypes ConditionType,
        float? Threshold,
        string? Unit,
        List<string>? Reasons            // Auto-populated based on Parameter
 );

public record AlertActionDto(
   Types Type,
   List<string> Recipients,
   string Message
);
