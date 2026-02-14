namespace MMS.Application.Ports.In.User.Validator;

public class AddUserDtoValidator : AbstractValidator<AddUserDto>
{
    public AddUserDtoValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.");

        RuleFor(x => x.Email)
          .NotEmpty().WithMessage("Email is required.")
          .MaximumLength(255).WithMessage("Email cannot exceed 255 characters.")
          .Matches(EmailRegex).WithMessage("Email must be a valid format (e.g., user@example.com).");

        When(x => !string.IsNullOrWhiteSpace(x.FirstName), () =>
        {
            RuleFor(x => x.FirstName)
                .MaximumLength(100).WithMessage("FirstName cannot exceed 50 characters.");
        });

        When(x => !string.IsNullOrWhiteSpace(x.LastName), () =>
        {
            RuleFor(x => x.LastName)
                .MaximumLength(100).WithMessage("LastName cannot exceed 50 characters.");
        });

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters long.");

        When(x => x.CustomerIds != null && x.CustomerIds.Any(), () =>
        {
            RuleForEach(x => x.CustomerIds!)
                .NotEmpty().WithMessage("Customer ID cannot be empty.");
        });

        When(x => x.FcmTokens != null && x.FcmTokens.Any(), () =>
        {
            RuleForEach(x => x.FcmTokens)
                .Must(fcm =>
                {
                    var parts = fcm.Split(new[] { "||" }, StringSplitOptions.None);
                    if (parts.Length < 2 || string.IsNullOrWhiteSpace(parts[0]))
                        return false; // Require "||" delimiter and non-empty deviceId

                    var deviceId = parts[0];
                    var fcmToken = parts[1]; // fcmToken can be empty

                    return deviceId.Length <= 100 && (string.IsNullOrEmpty(fcmToken) || fcmToken.Length <= 300);
                })
                .WithMessage("Each FCM token must be in the format 'deviceId||fcmToken' with a deviceId (max 100 characters) and an optional fcmToken (max 300 characters).");
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

        When(x => !string.IsNullOrWhiteSpace(x.City), () =>
        {
            RuleFor(x => x.City).MaximumLength(100);
        });

        When(x => !string.IsNullOrWhiteSpace(x.Country), () =>
        {
            RuleFor(x => x.Country).MaximumLength(100);
        });

        When(x => !string.IsNullOrWhiteSpace(x.Region), () =>
        {
            RuleFor(x => x.Region).MaximumLength(100);
        });

        When(x => !string.IsNullOrWhiteSpace(x.State), () =>
        {
            RuleFor(x => x.State).MaximumLength(100);
        });

        When(x => !string.IsNullOrWhiteSpace(x.TimeZone), () =>
        {
            RuleFor(x => x.TimeZone)
                .Matches(@"^UTC[+-](0[0-9]|1[0-4]):[0-5][0-9]$")
                .WithMessage("TimeZone must be in the format 'UTC±HH:MM' (e.g., UTC+05:00, UTC-03:30).");
        });

        When(x => !string.IsNullOrWhiteSpace(x.JobTitle), () =>
        {
            RuleFor(x => x.JobTitle).MaximumLength(100);
        });

        When(x => !string.IsNullOrWhiteSpace(x.Department), () =>
        {
            RuleFor(x => x.Department).MaximumLength(100);
        });

        When(x => x.Language.HasValue, () =>
        {
            RuleFor(x => x.Language)
                .IsInEnum()
                .WithMessage("Invalid language selected.");
        });

        When(x => !string.IsNullOrWhiteSpace(x.Role), () =>
        {
            RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required.")
            .Must(BeAValidRole).WithMessage("Invalid role provided.");
        });

    }

    private const string EmailRegex =
   @"^[^@\s]+@[^@\s]+\.[a-zA-Z]{2,}$";
    private bool BeAValidRole(string? role)
    {
        if (string.IsNullOrWhiteSpace(role)) return false;
        var validRoles = new[]
        {
            ApplicationRoles.RoleCustomerAdmin,
            ApplicationRoles.RoleOperator,
            ApplicationRoles.RoleTechnician,
            ApplicationRoles.RoleViewer,
            ApplicationRoles.RoleMMSBridge
        };

        return validRoles.Contains(role);
    }
}