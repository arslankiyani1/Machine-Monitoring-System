namespace MMS.Application.Ports.In.Widget.Validator;

public class UpdateWidgetDtoValidator : AbstractValidator<UpdateWidgetDto>
{
    public UpdateWidgetDtoValidator()
    {
        RuleFor(x => x.WidgetType)
            .IsInEnum()
            .WithMessage("Invalid widget type.");

        RuleFor(x => x.SourceType)
            .IsInEnum()
            .WithMessage("Invalid source type.");
    }
}
