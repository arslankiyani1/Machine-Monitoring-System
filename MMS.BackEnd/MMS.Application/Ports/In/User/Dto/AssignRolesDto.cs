namespace MMS.Application.Ports.In.User.Dto;
public record AssignRolesDto(
    Guid UserId,
    String Role
 );