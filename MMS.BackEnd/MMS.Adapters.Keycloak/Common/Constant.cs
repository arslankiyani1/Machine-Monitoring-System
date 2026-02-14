namespace MMS.Adapters.Keycloak.Common;

public static class Constant
{
    public static readonly string password_grant_type = "password";
    public static readonly string refresh_token_grant_type = "refresh_token";
    public static readonly string scope = "openid profile email";
    public static readonly string keyClockAdminRole = "admin";
    public static readonly string keyCloakDefaultRole = "default-roles-master";
}