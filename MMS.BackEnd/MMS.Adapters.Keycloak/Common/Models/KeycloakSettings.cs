namespace MMS.Adapters.Keycloak.Common.Models;

public class KeycloakSettings
{
    public string Realm { get; set; } = default!;
    public string KeyCloakUrl { get; set; } = default!;
    public string ClientId { get; set; } = default!;
    public string AdminUserName { get; set; } = default!;
    public string AdminPassword { get; set; } = default!;

    public string Authority => $"{KeyCloakUrl.TrimEnd('/')}/realms/{Realm}";
    public string TokenEndpoint => $"{KeyCloakUrl.TrimEnd('/')}/realms/{Realm}/protocol/openid-connect/token";
    public string LogoutEndpoint => $"{KeyCloakUrl.TrimEnd('/')}/realms/{Realm}/protocol/openid-connect/logout";
    public string UsersEndpoint => $"{KeyCloakUrl.TrimEnd('/')}/admin/realms/{Realm}/users";
    public string RolesEndpoint => $"{KeyCloakUrl.TrimEnd('/')}/admin/realms/{Realm}/roles";
    public string ExecuteActionsEndpoint => $"{KeyCloakUrl.TrimEnd('/')}/admin/realms/{Realm}/users";

}