namespace MMS.Application.Ports.In.MachineMaintenenceTask.Validator;

public class AddMachineMaintenanceTaskDtoValidator : AbstractValidator<AddMachineMaintenanceTaskDto>
{
    public AddMachineMaintenanceTaskDtoValidator()
    {
        RuleFor(x => x.MaintenanceTaskName)
            .NotEmpty()
            .WithMessage("Maintenance task name is required.")
            .MaximumLength(100)
            .WithMessage("Maintenance task name must not exceed 100 characters.");

        RuleFor(x => x.Category)
            .IsInEnum()
            .WithMessage("Invalid maintenance task category.");

        RuleFor(x => x.MachineId)
            .NotEmpty()
            .WithMessage("Machine ID is required.");

        //RuleFor(x => x.EndTime)
        //    .GreaterThanOrEqualTo(x => x.StartTime)
        //    .When(x => x.EndTime)
        //    .WithMessage("End time must be after start time.");
    }
}