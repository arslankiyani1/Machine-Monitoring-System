
namespace MMS.Application.Ports.In.Customer.Validator;

public class UpdateCustomerDtoValidator : AbstractValidator<UpdateCustomerDto>
{
    public UpdateCustomerDtoValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Invalid customer status.");

        RuleFor(x => x.Name)
        .NotEmpty().WithMessage("Name is required.")
        .MaximumLength(150).WithMessage("Name cannot exceed 150 characters.");

        RuleFor(x => x.Email)
    .NotEmpty().WithMessage("Email is required.")
    .MaximumLength(255).WithMessage("Email cannot exceed 255 characters.")
    .Matches(EmailRegex).WithMessage("Email must be a valid format (e.g., user@example.com).");


        RuleFor(x => x.TimeZone)
            .Matches(@"^UTC[+-](0[0-9]|1[0-4]):[0-5][0-9]$")
            .WithMessage("TimeZone must be in the format 'UTC±HH:MM' (e.g., UTC+05:00, UTC-03:30).");


        RuleFor(x => x.PhoneNumber)
        .NotEmpty().WithMessage("Phone number is required.")
        .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters.")
        .Matches(@"^[0-9]{6,15}$").WithMessage("Phone number must contain only digits (6–15).");


        RuleFor(x => x.PhoneCountryCode)
        .NotEmpty().WithMessage("Phone country code is required.")
        .Matches(@"^\+\d{1,3}$").WithMessage("Phone country code must be a valid format (e.g., +1, +44).");


    }

    private const string EmailRegex =
       @"^[^@\s]+@[^@\s]+\.[a-zA-Z]{2,}$";
}