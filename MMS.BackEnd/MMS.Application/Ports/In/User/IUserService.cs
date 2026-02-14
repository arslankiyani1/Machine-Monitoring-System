namespace MMS.Application.Ports.In.User;
public interface IUserService
{
    Task<ApiResponse<List<UserModel>>> GetAllUsersAsync(UserQueryParameters query,bool showAdmin);
    Task<ApiResponse<List<UserModel>>> GetAllUsersforNotification(UserQueryParameters query);
    Task<ApiResponse<UserModel>> GetUserByIdAsync(Guid userId);
    Task<ApiResponse<List<RoleDto>>> GetAllRealmRolesAsync();
    Task<ApiResponse<string>> CreateUserAsync(AddUserDto dto, bool isSignUp = true);
    Task<ApiResponse<string>> AssignRoleToUserAsync(AssignRolesDto dto);
    Task<ApiResponse<string>> VerifyEmailAsync(Guid userId);
    Task<ApiResponse<string>> EnableUserAsync(Guid userId);
    Task<ApiResponse<string>> DisableUserAsync(Guid userId);
    Task<ApiResponse<string>> ResetPasswordAsync(Guid userId, string newPassword);
    Task<ApiResponse<string>> UpdateUserAsync(Guid id, UpdateUserDto dto);
    Task<ApiResponse<string>> DeleteUserAsync(Guid id);
    Task<ApiResponse<UserModel>> GetCurrentUserAsync();
    AddUserDto ConvertToAddUserDto(SignUpUserDto signUpUserDto);
    Task<ApiResponse<IEnumerable<string>>> GetAccessibleCustomerIdsAsync(Guid userId);
    Task<ApiResponse<CustomerUsersSummaryDto>> GetCustomerUsersSummary(Guid customerId);
    Task<ApiResponse<string>> UpdatePasswordAsync(Guid userId, string oldPassword, string newPassword);
}
