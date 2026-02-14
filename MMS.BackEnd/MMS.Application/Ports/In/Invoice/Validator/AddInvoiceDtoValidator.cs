namespace MMS.Application.Ports.In.Invoice.Validator;

public class AddInvoiceDtoValidator : AbstractValidator<AddInvoiceDto>
{
    public AddInvoiceDtoValidator()
    {
        RuleFor(x => x.Invoicenumber)
            .NotEmpty().WithMessage("Invoice number is required.")
            .MaximumLength(50);

        RuleFor(x => x.Payment)
            .NotEmpty().WithMessage("Payment date is required.");

        RuleFor(x => x.Amout)
            .GreaterThanOrEqualTo(0).WithMessage("Amount must be non-negative.");

        RuleFor(x => x.Tax)
            .GreaterThanOrEqualTo(0).WithMessage("Tax must be non-negative.");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required.");

        RuleFor(x => x.Paymentmethod)
            .NotEmpty().WithMessage("Payment method is required.");

        RuleFor(x => x.PaymentGatewayTrxId)
            .NotEmpty().WithMessage("Payment gateway transaction ID is required.");

        RuleFor(x => x.CustomerSubscriptionId)
            .NotEmpty().WithMessage("Customer Subscription ID is required.");

        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required.");

        RuleFor(x => x.BillingAdressId)
            .NotEmpty().WithMessage("Billing Address ID is required.");
    }
}