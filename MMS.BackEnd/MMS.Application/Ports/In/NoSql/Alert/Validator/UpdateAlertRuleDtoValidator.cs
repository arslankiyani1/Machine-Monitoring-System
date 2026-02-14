namespace MMS.Application.Ports.In.NoSql.Alert.Validator;

public class UpdateAlertRuleDtoValidator : AbstractValidator<UpdateAlertRuleDto>
{
    public UpdateAlertRuleDtoValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required for update.");

        RuleFor(x => x.MachineId)
            .NotEmpty().WithMessage("MachineId is required.");

        RuleFor(x => x.RuleName)
            .MaximumLength(100).WithMessage("Rule name must not exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.RuleName));

        RuleFor(x => x.Conditions)
            .Must(c => c == null || c.Any()).WithMessage("If provided, at least one condition must be defined.")
            .ForEach(c => c.SetValidator(new ConditionDtoValidator()));

        RuleFor(x => x.AlertActions)
            .Must(a => a == null || a.Any()).WithMessage("If provided, at least one alert action must be defined.")
            .ForEach(a => a.SetValidator(new AlertActionDtoValidator()));

        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("Invalid priority level.")
            .When(x => x.Priority != null);

        RuleFor(x => x.Logic)
            .IsInEnum().WithMessage("Invalid logic type.")
            .When(x => x.Logic != null);
    }
}
