namespace MMS.Application.Ports.In.User.Validator;

public class AssignRolesDtoValidator : AbstractValidator<AssignRolesDto>
{
    public AssignRolesDtoValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required.")
            .Must(BeAValidRole).WithMessage("Invalid role provided.");
    }

    private bool BeAValidRole(string role)
    {
        var validRoles = new[]
        {
           // ApplicationRoles.RoleSystemAdmin,
            ApplicationRoles.RoleCustomerAdmin,
            ApplicationRoles.RoleOperator,
            ApplicationRoles.RoleTechnician,
            ApplicationRoles.RoleViewer,
            ApplicationRoles.RoleMMSBridge
        };

        return validRoles.Contains(role);
    }
}