namespace MMS.Adapters.Keycloak.Repositories;

public class UserContextService(IHttpContextAccessor httpContextAccessor) : IUserContextService
{
    private readonly ClaimsPrincipal? _user = httpContextAccessor.HttpContext?.User;

    public Guid? UserId =>
        Guid.TryParse(_user?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
    public string? Email => _user?.FindFirstValue(ClaimTypes.Email);
    public IEnumerable<string> CustomerIds =>
    _user?.FindAll("customerId").Select(c => c.Value) ?? Enumerable.Empty<string>();

    public string Role =>
    _user?.FindAll(ClaimTypes.Role)
          .Select(r => r.Value)
          .FirstOrDefault(role => ApplicationRoles.AllRoles.Contains(role))
    ?? ApplicationRoles.RoleViewer;

    public string userToken =>
        httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString().Replace("Bearer ", "") ?? string.Empty;

}