namespace MMS.Application.Ports.In.MachineSetting.Validator;

public class AddMachineSettingDtoValidator : AbstractValidator<AddMachineSettingDto>
{
    public AddMachineSettingDtoValidator()
    {
        RuleFor(x => x.CycleStartInterlock)
            .NotNull().WithMessage("CycleStartInterlock cannot be null.");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid status value.");

        RuleFor(x => x.MachineId)
            .NotEmpty().WithMessage("MachineId cannot be empty.");

        RuleFor(x => x.DownTimeReasons)
            .NotNull().WithMessage("DownTimeReasons cannot be null.")
            .Must(x => x.Count > 0).WithMessage("At least one downtime reason is required.");

    }
}