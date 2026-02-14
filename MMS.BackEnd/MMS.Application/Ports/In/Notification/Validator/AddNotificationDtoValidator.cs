namespace MMS.Application.Ports.In.Notification.Validator;

public class AddNotificationDtoValidator : AbstractValidator<AddNotificationDto>
{
    public AddNotificationDtoValidator()
    {
        RuleFor(dto => dto.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.");

        //RuleFor(dto => dto.Body)
        //    .NotEmpty().WithMessage("Body is required.");

        //RuleFor(dto => dto.DataPayload)
        //    .NotEmpty().WithMessage("Data payload is required.");

        //RuleFor(dto => dto.UserIds)
        //    .NotNull().WithMessage("UserIds is required.")
        //    .Must(u => u.Any()).WithMessage("At least one UserId must be provided.");

        //RuleForEach(dto => dto.UserIds)
        //    .Must(uid => uid == null || uid != Guid.Empty)
        //    .WithMessage("UserId cannot be an empty GUID.");
    }
}
