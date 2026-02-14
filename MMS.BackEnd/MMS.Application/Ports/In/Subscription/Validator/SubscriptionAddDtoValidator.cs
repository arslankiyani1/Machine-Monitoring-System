namespace MMS.Application.Ports.In.Subscription.Validator;

public class SubscriptionAddDtoValidator : AbstractValidator<SubscriptionAddDto>
{
    public SubscriptionAddDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price must be zero or positive.");

        RuleFor(x => x.BillingCycle)
            .IsInEnum().WithMessage("Invalid billing cycle.");

        RuleFor(x => x.Features)
            .NotNull().WithMessage("Features cannot be null.")
            .Must(features => features != null && features.Count > 0)
            .WithMessage("At least one feature must be specified.");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid status value."); 
    }
}
