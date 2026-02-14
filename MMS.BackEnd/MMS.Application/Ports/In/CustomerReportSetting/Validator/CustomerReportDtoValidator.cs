namespace MMS.Application.Ports.In.CustomerReportSetting.Validator;

public class CustomerReportDtoValidator : AbstractValidator<AddCustomerReportSettingDto>
{
    public CustomerReportDtoValidator()
    {
        RuleFor(x => x.ReportName)
            .NotEmpty().WithMessage("Report name is required.")
            .MaximumLength(100).WithMessage("Report name cannot exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("At least one email address is required.")
            .ForEach(email => email
                .NotEmpty().WithMessage("Email address cannot be empty.")
                .EmailAddress().WithMessage("Invalid email address format."));

        RuleFor(x => x.ReportType)
            .NotEmpty().WithMessage("At least one report type is required.")
            .ForEach(reportType => reportType
                .IsInEnum().WithMessage("Invalid report type."));

        RuleFor(x => x.Format)
            .IsInEnum().WithMessage("Invalid report format.");

        RuleFor(x => x.Frequency)
            .IsInEnum().WithMessage("Invalid report frequency.");

        RuleFor(x => x.MachineIds)
            .NotEmpty().WithMessage("At least one machine ID is required.")
            .ForEach(id => id
                .NotEmpty().WithMessage("Machine ID cannot be empty."));

        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required.");
    }
}