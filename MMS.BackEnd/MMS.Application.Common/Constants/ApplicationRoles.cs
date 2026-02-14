public static class ApplicationRoles
{
    public const string User = nameof(User);
    public const string CorporateAdmin = nameof(CorporateAdmin);
    public const string SystemAdmin = nameof(SystemAdmin);
    public const string UserId = "d32fa26e-0ed0-4054-a52d-66bae68cd0af";

    // Keycloak role values 
    public const string RoleSystemAdmin = "mms-superadmin";
    public const string RoleCustomerAdmin = "mms-customeradmin";
    public const string RoleOperator = "mms-operator";
    public const string RoleTechnician = "mms-technician";
    public const string RoleViewer = "mms-viewer";
    public const string RoleMMSBridge = "mms-bridge";

    public static readonly List<string> AllRoles = new()
    {
        RoleSystemAdmin,
        RoleCustomerAdmin,
        RoleOperator,
        RoleTechnician,
        RoleViewer,
        RoleMMSBridge,
   

    };
}
