namespace MMS.Application.Ports.In.Support.Validator;

public class AddSupportTicketDtoValidator : AbstractValidator<AddSupportTicketDto>
{
    public AddSupportTicketDtoValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.");

        When(x => x.Attachment != null, () =>
        {
            RuleFor(x => x.Attachment!.Length)
                .LessThanOrEqualTo(5 * 1024 * 1024) // 5MB
                .WithMessage("Attachment must be less than or equal to 5MB.");

            RuleFor(x => x.Attachment!.ContentType)
                .Must(ct => ct == "application/pdf" || ct.StartsWith("image/"))
                .WithMessage("Only PDF or image files are allowed.");
        });
    }
}
