namespace MMS.Application.Ports.In.MachineSensor.Validator;

public class AddMachineSensorDtoValidator : AbstractValidator<AddMachineSensorDto>
{
    public AddMachineSensorDtoValidator()
    {
        RuleFor(x => x.SerialNumber)
            .NotEmpty().WithMessage("Serial number is required.")
            .MinimumLength(6).WithMessage("Serial number must be at least 6 characters long.")
            .MaximumLength(50).WithMessage("Serial number cannot exceed 50 characters.")
            .Matches(@"^[A-Z0-9-]{6,50}$").WithMessage("Serial number must contain only uppercase letters, numbers, and dashes (e.g., SN-12345 or ABCD-9876).")
            .Must(value => !new[] { "string", "test", "demo", "sample", "abc123" }
                .Contains(value.Trim(), StringComparer.OrdinalIgnoreCase))
            .WithMessage("Invalid serial number — placeholder or test values are not allowed.");


        // ✅ Name validation
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Sensor name is required.")
            .MaximumLength(150).WithMessage("Sensor name cannot exceed 150 characters.");

        // ✅ Interface & SensorType should be valid enums
        RuleFor(x => x.Interface)
            .IsInEnum().WithMessage("Invalid sensor interface type.");

        RuleFor(x => x.SensorType)
            .IsInEnum().WithMessage("Invalid sensor type.");

        // ✅ Modbus IP format (optional)
        RuleFor(x => x.ModbusIp)
            .Matches(@"^(?:[0-9]{1,3}\.){3}[0-9]{1,3}$")
            .When(x => !string.IsNullOrEmpty(x.ModbusIp))
            .WithMessage("Invalid Modbus IP format.");

        // ✅ HRegList validation
        RuleFor(x => x.HRegList)
            .NotNull().WithMessage("Register list cannot be null.")
            .Must(list => list.Any()).WithMessage("At least one register is required.");

        // ✅ Image URL validation (optional)
        RuleFor(x => x.ImageUrl)
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
            .When(x => !string.IsNullOrEmpty(x.ImageUrl))
            .WithMessage("ImageUrl must be a valid URL.");
    }
}
