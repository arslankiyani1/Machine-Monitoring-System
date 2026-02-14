namespace MMS.Application.Ports.In.MachineMaintenenceTask.Validator;

public class UpdateMachineMaintenanceTaskDtoValidator : AbstractValidator<UpdateMachineMaintenanceTaskDto>
{
    public UpdateMachineMaintenanceTaskDtoValidator()
    {
        RuleFor(x => x.Category)
           .IsInEnum()
           .WithMessage("Invalid maintenance task category.");
    }
}
