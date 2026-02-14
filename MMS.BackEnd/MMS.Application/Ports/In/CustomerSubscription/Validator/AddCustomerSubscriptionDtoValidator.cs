namespace MMS.Application.Ports.In.CustomerSubscription.Validator;

public class AddCustomerSubscriptionDtoValidator : AbstractValidator<AddCustomerSubscriptionDto>
{
    public AddCustomerSubscriptionDtoValidator()
    {
        RuleFor(x => x.StartDate)
           .NotEmpty().WithMessage("Start date is required.")
           .LessThan(x => x.EndDate).WithMessage("Start date must be before end date.");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required.")
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date.");

        RuleFor(x => x.RenewalType)
            .IsInEnum().WithMessage("Invalid renewal type.");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid subscription status.");

        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required.");

        RuleFor(x => x.SubscriptionId)
            .NotEmpty().WithMessage("Subscription ID is required.");
    }
}
