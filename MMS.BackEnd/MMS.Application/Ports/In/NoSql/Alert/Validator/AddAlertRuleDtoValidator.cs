namespace MMS.Application.Ports.In.NoSql.Alert.Validator;

public class AddAlertRuleDtoValidator : AbstractValidator<AddAlertRuleDto>
{
    public AddAlertRuleDtoValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("CustomerId is required.");

        RuleFor(x => x.MachineId)
            .NotEmpty().WithMessage("MachineId is required.");

        RuleFor(x => x.RuleName)
            .NotEmpty().WithMessage("Rule name is required.")
            .MaximumLength(100).WithMessage("Rule name must not exceed 100 characters.");

        RuleFor(x => x.Conditions)
            .NotNull().WithMessage("At least one condition is required.")
            .Must(c => c.Any()).WithMessage("At least one condition must be provided.")
            .ForEach(c => c.SetValidator(new ConditionDtoValidator()));

        RuleFor(x => x.AlertActions)
            .NotNull().WithMessage("At least one alert action is required.")
            .Must(a => a.Any()).WithMessage("At least one alert action must be provided.")
            .ForEach(a => a.SetValidator(new AlertActionDtoValidator()));

        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("Invalid priority level.");

        RuleFor(x => x.Logic)
            .IsInEnum().WithMessage("Invalid logic type.");
    }
}

public class ConditionDtoValidator : AbstractValidator<ConditionDto>
{
    // Define allowed units per parameter
    private static readonly Dictionary<ParameterType, string[]> ParameterUnits = new()
        {
            { ParameterType.Temperature, new[] { "°C", "°F" , "K" } },
            { ParameterType.Vibration, new[] { "mm/s", "µm", "g" } },
            { ParameterType.CoolantLevel, new[] { "L", "%" } },
            { ParameterType.AirPressure, new[] { "bar", "psi" } },
        };

    public ConditionDtoValidator()
    {
        RuleFor(x => x.Parameter)
            .IsInEnum().WithMessage("Invalid parameter type.");

        RuleFor(x => x.ConditionType)
            .IsInEnum().WithMessage("Invalid condition type.");

        RuleFor(x => x.Threshold)
            .GreaterThanOrEqualTo(0).WithMessage("Threshold must be non-negative.");
    }
}
public class AlertActionDtoValidator : AbstractValidator<AlertActionDto>
{
    public AlertActionDtoValidator()
    {
        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Invalid alert action type.");

        When(x => x.Type == Types.email, () =>
        {
            RuleFor(x => x.Recipients)
                .NotNull().WithMessage("Recipients list is required for email alerts.")
                .Must(r => r.Any()).WithMessage("At least one recipient must be provided for email alerts.")
                .ForEach(r => r
                    .NotEmpty().WithMessage("Recipient email is required.")
                    .EmailAddress().WithMessage("Recipient email must be a valid email address.")
                );
        });

        When(x => x.Type == Types.push, () =>
        {
            RuleFor(x => x.Recipients)
                .NotNull().WithMessage("Recipients list is required for push alerts.")
                .Must(r => r.Any()).WithMessage("At least one recipient must be provided for push alerts.")
                .ForEach(r => r
                    .Must(BeValidGuid)
                    .WithMessage("Each recipient for push alerts must be a valid GUID format.")
                );
        });

        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Message is required.")
            .MaximumLength(500).WithMessage("Message must not exceed 500 characters.");
    }

    private bool BeValidGuid(string recipient)
    {
        return Guid.TryParse(recipient, out _);
    }
}