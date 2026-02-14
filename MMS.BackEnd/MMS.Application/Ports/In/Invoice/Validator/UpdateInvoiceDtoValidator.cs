namespace MMS.Application.Ports.In.Invoice.Validator;

public class UpdateInvoiceDtoValidator : AbstractValidator<UpdateInvoiceDto>
{
    public UpdateInvoiceDtoValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Invoice ID is required.");
    }
}