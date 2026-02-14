namespace MMS.Application.Common.Validators;

[ExcludeFromCodeCoverage]
public class GuidValidator : AbstractValidator<Guid>
{
    public GuidValidator()
    {
        RuleFor(r => r).NotEqual(Guid.Empty)
            .WithMessage("Provided Guid value is an empty Guid");
    }
}