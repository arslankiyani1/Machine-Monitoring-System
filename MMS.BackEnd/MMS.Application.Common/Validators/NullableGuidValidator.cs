namespace MMS.Application.Common.Validators;

[ExcludeFromCodeCoverage]
public class NullableGuidValidator : AbstractValidator<Guid?>
{
    public NullableGuidValidator()
    {
        RuleFor(r => r).NotEqual(Guid.Empty)
            .WithMessage("Provided Guid value is an empty Guid")
            .When(guid => guid.HasValue);
    }
}