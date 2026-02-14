namespace MMS.Application.Ports.In.MachineSensor.Validator;

public class UpdateMachineSensorDtoValidator : AbstractValidator<UpdateMachineSensorDto>
{
    public UpdateMachineSensorDtoValidator()
    {
        // ✅ Ensure ID is required for update
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Sensor ID is required for update.");

        RuleFor(x => x.SerialNumber)
            .NotEmpty().WithMessage("Serial number is required.")
            .MinimumLength(6).WithMessage("Serial number must be at least 6 characters long.")
            .MaximumLength(50).WithMessage("Serial number cannot exceed 50 characters.")
            .Matches(@"^[A-Z0-9-]{6,50}$").WithMessage("Serial number must contain only uppercase letters, numbers, and dashes (e.g., SN-12345 or ABCD-9876).")
            .Must(value => !new[] { "string", "test", "demo", "sample", "abc123" }
                .Contains(value.Trim(), StringComparer.OrdinalIgnoreCase))
            .WithMessage("Invalid serial number — placeholder or test values are not allowed.");


        // ✅ MachineId optional (no 'NotEmpty' rule)
        RuleFor(x => x.MachineId)
            .Must(id => id == null || id != Guid.Empty)
            .WithMessage("If provided, MachineId must be a valid Guid.");

        // ✅ Optional name validation
        RuleFor(x => x.Name)
            .MaximumLength(150).WithMessage("Name cannot exceed 150 characters.")
            .When(x => !string.IsNullOrEmpty(x.Name));

        // ✅ Optional IP validation
        RuleFor(x => x.ModbusIp)
            .Matches(@"^(?:[0-9]{1,3}\.){3}[0-9]{1,3}$")
            .When(x => !string.IsNullOrEmpty(x.ModbusIp))
            .WithMessage("Invalid Modbus IP format.");

        // ✅ Optional HRegList entries validation
        RuleForEach(x => x.HRegList)
            .NotEmpty().WithMessage("HRegList entries cannot be empty.")
            .When(x => x.HRegList != null);

        // ✅ Optional image URL validation
        RuleFor(x => x.ImageUrl)
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
            .When(x => !string.IsNullOrEmpty(x.ImageUrl))
            .WithMessage("ImageUrl must be a valid URL.");
    }
}