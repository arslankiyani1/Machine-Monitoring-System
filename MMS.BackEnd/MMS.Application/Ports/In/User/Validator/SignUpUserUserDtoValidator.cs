namespace MMS.Application.Ports.In.User.Validator;

public class SignUpUserDtoValidator : AbstractValidator<SignUpUserDto>
{
    public SignUpUserDtoValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.");


        RuleFor(x => x.Email)
          .NotEmpty().WithMessage("Email is required.")
          .MaximumLength(255).WithMessage("Email cannot exceed 255 characters.")
          .Matches(EmailRegex).WithMessage("Email must be a valid format (e.g., user@example.com).");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters long.");


        RuleFor(x => x.CustomerIds)
        .NotNull().WithMessage("Customer IDs cannot be null.")
        .NotEmpty().WithMessage("At least one Customer ID is required.");

        When(x => x.CustomerIds != null && x.CustomerIds.Any(), () =>
        {
            RuleForEach(x => x.CustomerIds!)
                .NotEmpty().WithMessage("Customer ID cannot be empty.");
        });

        When(x => !string.IsNullOrWhiteSpace(x.PhoneCode), () =>
        {
            RuleFor(x => x.PhoneCode)
                .Matches(@"^\+\d{1,4}$")
                .WithMessage("Phone code must start with '+' followed by 1 to 4 digits (e.g., +92, +1, +968).");
        });

        When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber), () =>
        {
            RuleFor(x => x.PhoneNumber)
                .Matches(@"^\d{7,15}$")
                .WithMessage("Phone number must be 7 to 15 digits without '+' or spaces.");
        });

      
        When(x => x.Language.HasValue, () =>
        {
            RuleFor(x => x.Language)
                .IsInEnum()
                .WithMessage("Invalid language selected.");
        });

    }

    private const string EmailRegex = @"^[^@\s]+@[^@\s]+\.[a-zA-Z]{2,}$";

}