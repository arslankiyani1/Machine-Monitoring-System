namespace MMS.Application.Common.Interfaces;

public interface IUserContextService
{
    Guid? UserId { get; }
    string? Email { get; }
    IEnumerable<string>  CustomerIds { get; }
    string Role { get; }
    string userToken { get; }
}
