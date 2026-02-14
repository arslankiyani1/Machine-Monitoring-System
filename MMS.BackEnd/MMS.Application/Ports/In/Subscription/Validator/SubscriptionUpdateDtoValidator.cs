namespace MMS.Application.Ports.In.Subscription.Validator;

public class SubscriptionUpdateDtoValidator :AbstractValidator<SubscriptionUpdateDto>
{
    public SubscriptionUpdateDtoValidator()
    {
        RuleFor(x => x.Status)
           .IsInEnum().WithMessage("Invalid status value.");
    }
}
