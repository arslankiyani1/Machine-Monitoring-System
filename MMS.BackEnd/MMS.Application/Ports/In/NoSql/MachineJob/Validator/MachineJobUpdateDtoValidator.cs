using MMS.Application.Ports.In.NoSql.MachineJob.Dto;

namespace MMS.Application.Ports.In.NoSql.MachineJob.Validator;

public class MachineJobUpdateDtoValidator : AbstractValidator<MachineJobUpdateDto>
{
    public MachineJobUpdateDtoValidator()
    {
        // ✅ Id Validation
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Id is required for updating a job.");

        // ✅ Customer Validation
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("CustomerId is required.");

        // ✅ Machine Validation
        RuleFor(x => x.MachineIds)
            .NotEmpty()
            .WithMessage("At least one MachineId is required.");

        RuleForEach(x => x.MachineIds)
            .NotEmpty()
            .WithMessage("MachineId cannot be empty.");

        RuleFor(x => x.MachineNames)
            .NotEmpty()
            .WithMessage("At least one MachineName is required.");

        // ✅ Operator Validation
        RuleFor(x => x.OperatorId)
            .NotEmpty()
            .WithMessage("OperatorId is required.");

        RuleFor(x => x.OperatorName)
            .NotEmpty()
            .WithMessage("OperatorName is required.")
            .MaximumLength(100);

        // ✅ Job Information
        RuleFor(x => x.PartNumber)
            .NotEmpty()
            .WithMessage("PartNumber is required.")
            .MaximumLength(100);

        RuleFor(x => x.ProgramNo)
            .NotEmpty()
            .WithMessage("ProgramNo is required.");

        RuleFor(x => x.MainProgram)
            .NotEmpty()
            .WithMessage("MainProgram is required.");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.OrderNo)
            .NotEmpty()
            .WithMessage("OrderNo is required.");

        RuleFor(x => x.OrderDate)
            .LessThanOrEqualTo(x => DateTime.UtcNow)
            .WithMessage("OrderDate cannot be in the future.");

        RuleFor(x => x.DueDate)
            .GreaterThan(x => x.OrderDate)
            .WithMessage("DueDate must be after OrderDate.");

        // ✅ Job Time Validation
        RuleFor(x => x.StartTime)
            .LessThan(x => x.EndTime)
            .WithMessage("StartTime must be before EndTime.");

        // ✅ Quantities Validation
        RuleFor(x => x.Quantities)
            .NotNull()
            .WithMessage("Quantities object is required.");

        RuleFor(x => x.Quantities.Required)
            .GreaterThanOrEqualTo(0);

        // ✅ Metrics Validation
        RuleFor(x => x.Metrics)
            .NotNull()
            .WithMessage("Metrics object is required.");

        // ✅ Schedule Validation
        RuleFor(x => x.Schedule)
            .NotNull()
            .WithMessage("Schedule object is required.");

        RuleFor(x => x.Schedule.PlannedEnd)
            .GreaterThan(x => x.Schedule.PlannedStart)
            .WithMessage("PlannedEnd must be after PlannedStart.");

        // ✅ Setup Validation
        RuleFor(x => x.Setup)
            .NotNull()
            .WithMessage("Setup object is required.");

        // ✅ Priority & Type Validation
        RuleFor(x => x.PriorityLevel)
            .IsInEnum()
            .WithMessage("Invalid PriorityLevel.");

        RuleFor(x => x.JobType)
            .IsInEnum()
            .WithMessage("Invalid JobType.");

        // ✅ Downtime Events Validation
        RuleForEach(x => x.DowntimeEvents)
            .ChildRules(eventRule =>
            {
                eventRule.RuleFor(e => e.StartTime)
                    .LessThan(e => e.EndTime)
                    .WithMessage("Downtime StartTime must be before EndTime.");

                eventRule.RuleFor(e => e.Duration)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage("Downtime duration must be non-negative.");
            });

        //// ✅ Notes
        //RuleFor(x => x.Notes)
        //    .MaximumLength(500)
        //    .When(x => !string.IsNullOrEmpty(x.Notes));

        //// ✅ File Upload Validation (optional)
        //When(x => x.File != null, () =>
        //{
        //    RuleFor(x => x.File.Length)
        //        .LessThanOrEqualTo(10 * 1024 * 1024)
        //        .WithMessage("File size must not exceed 10 MB.");
        //});
    }
}
