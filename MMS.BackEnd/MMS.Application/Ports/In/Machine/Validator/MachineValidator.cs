namespace MMS.Application.Ports.In.Machine.Validator;

public class AddMachineValidator : AbstractValidator<AddMachineDto>
{
    public AddMachineValidator()
    {
        RuleFor(x => x.MachineName)
            .NotEmpty().WithMessage("Machine Name is required.")
            .MaximumLength(100).WithMessage("Machine Name cannot exceed 100 characters.");

        RuleFor(x => x.MachineModel)
            .NotEmpty().WithMessage("Machine Model is required.")
            .MaximumLength(50).WithMessage("Machine Model cannot exceed 50 characters.");

        RuleFor(x => x.Manufacturer).NotEmpty()
            .MaximumLength(100).WithMessage("Manufacturer name cannot exceed 100 characters.");

        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("CustomerId is required.");

        RuleFor(x => x.SerialNumber)
           .NotEmpty().WithMessage("Serial Number is required.");
           //.MaximumLength(50).WithMessage("Serial Number cannot exceed 50 characters.")
           //.Matches(@"^[a-zA-Z0-9\-]+$").WithMessage("Serial Number can only contain alphanumeric characters and dashes.");

        RuleFor(x => x.CommunicationProtocol)
                .IsInEnum().WithMessage("Invalid communication protocol.");

        RuleFor(x => x.MachineType)
            .IsInEnum().WithMessage("Invalid machine type.");

    }
}
