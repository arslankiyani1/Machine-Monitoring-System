namespace MMS.Application.Ports.In.CustomerDashboard.Validator;

public class CustomerDashboardValidator
{
    public class Add : AbstractValidator<AddCustomerDashboardDto>
    {
        public Add()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.");

            RuleFor(x => x.Theme)
                .IsInEnum().WithMessage("Invalid dashboard theme.");

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Invalid dashboard status.");
        }
    }
}
