using MMS.Application.Ports.In.CustomerReportSetting.Dto;

namespace MMS.Application.Ports.In.CustomerReportSetting.Validator;

public class UpdateCustomerReportDtoValidator : AbstractValidator<UpdateCustomerReportSettingDto>
{
    public UpdateCustomerReportDtoValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("CustomerReport ID is required.");

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

        RuleFor(x => x.WeekDays)
            .NotEmpty().When(x => x.Frequency == ReportFrequency.Weekly)
            .WithMessage("At least one weekday is required for weekly reports.")
            .ForEach(day => day
                .IsInEnum().WithMessage("Invalid weekday."));

        RuleFor(x => x.ReportPeriodStartDate)
            .NotEmpty().WithMessage("Report period start date is required.")
            .LessThanOrEqualTo(x => x.ReportPeriodEndDate)
            .WithMessage("Start date must be before or equal to end date.");

        RuleFor(x => x.ReportPeriodEndDate)
            .NotEmpty().WithMessage("Report period end date is required.")
            .GreaterThanOrEqualTo(x => x.ReportPeriodStartDate)
            .WithMessage("End date must be after or equal to start date.");

        RuleFor(x => x.MachineIds)
            .NotEmpty().WithMessage("At least one machine ID is required.")
            .ForEach(id => id
                .NotEmpty().WithMessage("Machine ID cannot be empty."));

        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required.");
    }
}