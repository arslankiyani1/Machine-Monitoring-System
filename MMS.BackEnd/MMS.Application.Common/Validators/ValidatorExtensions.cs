namespace MMS.Application.Common.Validators;

[ExcludeFromCodeCoverage]
[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public static class ValidatorExtensions
{
    public static IRuleBuilderOptions<T, string> MustBeValidGuid<T>(this IRuleBuilder<T, string> ruleBuilder) =>
        ruleBuilder.Must(s => Guid.TryParseExact(s, "D", out var guid) && guid != Guid.Empty)
            .WithMessage("'{PropertyValue}' is not a valid guid");
}