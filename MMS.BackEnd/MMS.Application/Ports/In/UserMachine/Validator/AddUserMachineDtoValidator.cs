namespace MMS.Application.Ports.In.UserMachine.Validator;

public class AddUserMachineDtoValidator : AbstractValidator<AddUserMachineDto>
{
    public AddUserMachineDtoValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");

        RuleFor(x => x.MachineId)
            .NotEmpty().WithMessage("MachineId is required.");
    }
}
