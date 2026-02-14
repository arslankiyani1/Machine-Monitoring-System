namespace MMS.Application.Ports.In.CustomerDashboard.Validator;

public class UpdateCustomerDashValidator : AbstractValidator<UpdateCustomerDashboardDto>
{
    public UpdateCustomerDashValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Invalid customer status.");
    }
}
