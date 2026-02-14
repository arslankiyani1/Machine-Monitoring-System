namespace MMS.Application.Ports.In.CustomerSubscription.Validator;

public class UpdateCustomerSubscriptionDtoValidator : AbstractValidator<UpdateCustomerSubscriptionDto>
{
    public UpdateCustomerSubscriptionDtoValidator()
    {
        RuleFor(x => x.RenewalType)
           .IsInEnum().WithMessage("Invalid renewal type.");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid subscription status.");
    }
}
