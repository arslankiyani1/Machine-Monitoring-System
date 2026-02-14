namespace MMS.Application.Utils;
public static class RoleTranslator
{
    private static readonly Dictionary<string, Dictionary<Language, string>> RoleTranslations = new()
    {
        [ApplicationRoles.RoleSystemAdmin] = new()
        {
            [Language.en] = "Super Admin",
            [Language.hi] = "सुपर एडमिन",
            [Language.de] = "Superadministrator",
            [Language.es] = "Superadministrador",
            [Language.fr] = "Super administrateur"
        },
        [ApplicationRoles.RoleCustomerAdmin] = new()
        {
            [Language.en] = "Customer Admin",
            [Language.hi] = "ग्राहक एडमिन",
            [Language.de] = "Kundenadministrator",
            [Language.es] = "Administrador del cliente",
            [Language.fr] = "Administrateur client"
        },
        [ApplicationRoles.RoleOperator] = new()
        {
            [Language.en] = "Operator",
            [Language.hi] = "ऑपरेटर",
            [Language.de] = "Bediener",
            [Language.es] = "Operador",
            [Language.fr] = "Opérateur"
        },
        [ApplicationRoles.RoleTechnician] = new()
        {
            [Language.en] = "Technician",
            [Language.hi] = "तकनीशियन",
            [Language.de] = "Techniker",
            [Language.es] = "Técnico",
            [Language.fr] = "Technicien"
        },
        [ApplicationRoles.RoleViewer] = new()
        {
            [Language.en] = "Viewer",
            [Language.hi] = "दर्शक",
            [Language.de] = "Betrachter",
            [Language.es] = "Espectador",
            [Language.fr] = "Spectateur"
        }
    };

    public static string? GetTranslatedRole(string? role, Language language)
    {
        if (String.IsNullOrWhiteSpace(role)){
            return null;
        }

        if (RoleTranslations.TryGetValue(role, out var translations) &&
            translations.TryGetValue(language, out var translated)){
            return translated;
        }

        // Fallback to English if translation not found
        return RoleTranslations.TryGetValue(role, out var enTranslations) &&
               enTranslations.TryGetValue(Language.en, out var fallback)
               ? fallback
               : role;
    }
}
