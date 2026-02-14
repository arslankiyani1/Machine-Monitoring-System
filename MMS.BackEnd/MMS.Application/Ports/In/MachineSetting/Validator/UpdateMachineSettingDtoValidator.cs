namespace MMS.Application.Ports.In.MachineSetting.Validator;

public class UpdateMachineSettingDtoValidator : AbstractValidator<UpdateMachineSettingDto>
{
    public UpdateMachineSettingDtoValidator()
    {
        RuleFor(x => x.Status)
             .IsInEnum()
             .WithMessage("The selected status is invalid. Please choose a valid status.");
    }
}
