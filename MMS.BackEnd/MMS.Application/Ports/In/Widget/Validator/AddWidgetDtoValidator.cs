namespace MMS.Application.Ports.In.Widget.Validator;

public class AddWidgetDtoValidator : AbstractValidator<AddWidgetDto>
{
    public AddWidgetDtoValidator()
    {
        RuleFor(x => x.WidgetType)
            .IsInEnum()
            .WithMessage("Invalid widget type.");

        RuleFor(x => x.SourceType)
            .IsInEnum()
            .WithMessage("Invalid source type.");

        RuleFor(x => x.Config)
            .NotNull()
            .WithMessage("Config must be provided.");

        RuleFor(x => x.DashboardId)
            .NotEmpty()
            .WithMessage("DashboardId must be a valid GUID.");
    }
}
