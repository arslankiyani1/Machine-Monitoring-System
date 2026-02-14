using MMS.Application.Common;

namespace MMS.Adapters.Keycloak.Repositories;

public class UserService(
    HttpClient httpClient,
    IBlobStorageService blobStorageService,
    IUserContextService userContext,
    IMachineRepository machineRepository,
    IUserMachineRepository userMachineRepository,
    IUnitOfWork unitOfWork,
    AutoMapperResult mapper,
    IOptions<KeycloakSettings> options,
    UserServiceHelper helper) : IUserService
{
    private readonly KeycloakSettings _keyCloakConfig = options.Value;

    public async Task<ApiResponse<UserModel>> GetCurrentUserAsync()
    {
        try
        {
            if (userContext.UserId == null || userContext.UserId == Guid.Empty)
            {
                return new ApiResponse<UserModel>(
                    StatusCodes.Status400BadRequest,
                    "Invalid or missing UserId.");
            }

            return await GetUserByIdAsync(userContext.UserId.Value);
        }
        catch (Exception ex)
        {
            return new ApiResponse<UserModel>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<UserModel>>> GetAllUsersAsync(UserQueryParameters query, bool showAdminAndCurrentUser)
    {
        try
        {
            var adminToken = await helper.GetAdminTokenAsync();
            var url = await helper.BuildUsersQueryUrlAsync(query);

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await helper.ExtractErrorMessageAsync(response);
                return new ApiResponse<List<UserModel>>((int)response.StatusCode, $"Failed to fetch users: {errorMessage}");
            }

            var content = await response.Content.ReadAsStringAsync();
            var usersJson = JsonSerializer.Deserialize<JsonElement>(content);
            var result = await helper.ProcessUsersResponseAsync(usersJson, adminToken, true, showAdminAndCurrentUser);

            // Enforce CustomerId locally (important when Term is combined with search)
            if (!string.IsNullOrWhiteSpace(query.CustomerId))
            {
                result = result
                    .Where(u => u.CustomerIds != null &&
                                u.CustomerIds.Contains(query.CustomerId))
                    .ToList();
            }
            return new ApiResponse<List<UserModel>>(200, "Users fetched successfully", result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return new ApiResponse<List<UserModel>>(
                StatusCodes.Status403Forbidden,
                ex.Message
            );
        }
        catch (Exception ex)
        {
            return new ApiResponse<List<UserModel>>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<UserModel>>> GetAllUsersforNotification(UserQueryParameters query)
    {
        try
        {
            var adminToken = await helper.GetAdminTokenAsync();
            var url = await helper.BuildUsersQueryUrlAsync(query);

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await helper.ExtractErrorMessageAsync(response);
                return new ApiResponse<List<UserModel>>((int)response.StatusCode, $"Failed to fetch users: {errorMessage}");
            }

            var content = await response.Content.ReadAsStringAsync();
            var usersJson = JsonSerializer.Deserialize<JsonElement>(content);
            var result = await helper.ProcessUsersResponseAsync(usersJson, adminToken, false, false);

          

            return new ApiResponse<List<UserModel>>(200, "Users fetched successfully", result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return new ApiResponse<List<UserModel>>(
                StatusCodes.Status403Forbidden,
                ex.Message
            );
        }
        catch (Exception ex)
        {
            return new ApiResponse<List<UserModel>>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred: {ex.Message}");
        }
    }

    public async Task<ApiResponse<UserModel>> GetUserByIdAsync(Guid userId)
    {
        try
        {
            var adminToken = await helper.GetAdminTokenAsync();
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_keyCloakConfig.UsersEndpoint}/{userId}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await helper.ExtractErrorMessageAsync(response);
                return new ApiResponse<UserModel>((int)response.StatusCode, errorMessage);
            }

            var content = await response.Content.ReadAsStringAsync();
            var user = JsonSerializer.Deserialize<JsonElement>(content);
            var roles = await helper.GetUserRolesAsync(userId, adminToken);
            var machines = await helper.GetUserMachinesAsync(userId);
            var userDto = UserServiceHelper.MapToUserDto(userId, user, roles, machines);
            //await CustomerAccessHelper.ValidateCustomerAccessAsync(userContext, userDto.CustomerIds);
            return new ApiResponse<UserModel>(200, "User fetched successfully", userDto);
        }
        catch (UnauthorizedAccessException ex)
        {
            return new ApiResponse<UserModel>(
                StatusCodes.Status403Forbidden,
                ex.Message
            );
        }
        catch (Exception ex)
        {
            return new ApiResponse<UserModel>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<RoleDto>>> GetAllRealmRolesAsync()
    {
        try
        {
            var token = await helper.GetAdminTokenAsync();

            if (userContext.UserId == null || userContext.UserId == Guid.Empty)
            {
                return new ApiResponse<List<RoleDto>>(
                    StatusCodes.Status400BadRequest,
                    "Invalid or missing UserId.");
            }

            var userRoles = await helper.GetUserRolesAsync((Guid)userContext.UserId, token);
            var isMssAdmin = userRoles.Any(r =>
                string.Equals(r.Name, ApplicationRoles.RoleSystemAdmin, StringComparison.OrdinalIgnoreCase));

            var request = new HttpRequestMessage(HttpMethod.Get, _keyCloakConfig.RolesEndpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return new ApiResponse<List<RoleDto>>((int)response.StatusCode, "Failed to fetch roles");

            var content = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content).RootElement;
            var roles = helper.FilterRealmRoles(json, isMssAdmin);

            return new ApiResponse<List<RoleDto>>(200, "Roles fetched", roles);
        }
        catch (Exception ex)
        {
            return new ApiResponse<List<RoleDto>>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred: {ex.Message}");
        }
    }

    public async Task<ApiResponse<string>> CreateUserAsync(AddUserDto dto, bool isSignup = true)
    {
        try
        {
            var adminToken = await helper.GetAdminTokenAsync();

            if (!isSignup)
            {
                var roleValidation = await helper.ValidateRealmRoleAsync(dto.Role, GetAllRealmRolesAsync);
                if (roleValidation.StatusCode != 200)
                    return roleValidation;
                var machineValidation = await validateAddUserMachines(dto.MachineIds);
                if (machineValidation.StatusCode != 200)
                    return machineValidation;
            }

            var checkResult = await helper.CheckCustomerIdsExistAsync(dto.CustomerIds ?? new());
            if (checkResult.StatusCode != StatusCodes.Status200OK)
                return checkResult;

            var profileImageUrl = await UploadProfileImageAsync(dto.ProfileImageBase64);

            var userPayload = helper.BuildCreateUserPayload(dto, profileImageUrl, isSignup);
            var response = await SendCreateUserRequestAsync(userPayload, adminToken);

            if (!response.IsSuccessStatusCode)
            {
                await helper.DeleteUserImageIfExistsAsync(profileImageUrl);
                var errorMessage = await helper.ExtractKeycloakErrorAsync(response);
                return new ApiResponse<string>((int)response.StatusCode, $"Keycloak Error: {errorMessage}");
            }
            var userIdResult = await helper.GetUserIdFromResponse(response);
            if (userIdResult.StatusCode != 200 || userIdResult.Data == null)
                return userIdResult;
            Guid userId = userIdResult!.Data.ToGuid();

            var assignResult = await helper.AssignRoleIfApplicableAsync(response, dto, isSignup, AssignRoleToUserAsync);
            if (assignResult.StatusCode != 200)
                return assignResult;

            var machineAssignResult = await addUserMachines(dto.MachineIds, userId);
            if (machineAssignResult.StatusCode != 200)
                return machineAssignResult;

            if (isSignup)
            {
                await VerifyEmailAsync(userId);
            }

            var message = helper.BuildCreateUserSuccessMessage(isSignup);
            return new ApiResponse<string>(201, message, userId.ToString());
        }
        catch (Exception ex)
        {
            return new ApiResponse<string>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred: {ex.Message}");
        }
    }

    public async Task<ApiResponse<string>> EnableUserAsync(Guid userId)
    {
        return await UpdateUserStatusAsync(userId, true, "enable");
    }

    public async Task<ApiResponse<string>> DisableUserAsync(Guid userId)
    {
        return await UpdateUserStatusAsync(userId, false, "disable");
    }

    public async Task<ApiResponse<string>> ResetPasswordAsync(Guid userId, string newPassword)
    {
        try
        {
            var adminToken = await helper.GetAdminTokenAsync();

            var payload = new
            {
                type = "password",
                value = newPassword,
                temporary = false
            };

            var request = new HttpRequestMessage(HttpMethod.Put,
                $"{_keyCloakConfig.UsersEndpoint}/{userId}/reset-password")
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await helper.ExtractErrorMessageAsync(response);
                return new ApiResponse<string>((int)response.StatusCode, $"Failed to reset user password: {errorMessage}");
            }

            return new ApiResponse<string>(StatusCodes.Status200OK, "Password reset successful.");
        }
        catch (Exception ex)
        {
            return new ApiResponse<string>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred: {ex.Message}");
        }
    }

    public async Task<ApiResponse<string>> UpdateUserAsync(Guid id, UpdateUserDto dto)
    {
        try
        {
            var token = await helper.GetAdminTokenAsync();

            var roleValidation = await helper.ValidateRealmRoleAsync(dto.Role, GetAllRealmRolesAsync);
            if (roleValidation.StatusCode != 200)
                return new ApiResponse<string>(roleValidation.StatusCode, roleValidation.Message);
            var machineValidation = await validateAddUserMachines(dto.MachineIds);
            if (machineValidation.StatusCode != 200)
                return machineValidation;

            var checkResult = await helper.CheckCustomerIdsExistAsync(dto.CustomerIds ?? new());
            if (checkResult.StatusCode != StatusCodes.Status200OK)
                return checkResult;

            var currentUserResponse = await GetCurrentUserDataAsync(id, token);
            if (!currentUserResponse.IsSuccessStatusCode)
                return new ApiResponse<string>((int)currentUserResponse.StatusCode, "User not found");

            var updatedImageUrl = await UploadProfileImageAsync(dto.ProfileImageBase64);
            var currentJson = JsonDocument.Parse(await currentUserResponse.Content.ReadAsStringAsync()).RootElement;
            var oldImageUrl = UserServiceHelper.GetOldImageUrl(currentJson);
            var updatedUser = UserServiceHelper.BuildUpdatedUserPayload(currentJson, dto, updatedImageUrl);

            var updateResult = await UpdateUserDataAsync(id, updatedUser, token);
            if (!updateResult.IsSuccessStatusCode)
            {
                await helper.DeleteUserImageIfExistsAsync(updatedImageUrl);
                return new ApiResponse<string>((int)updateResult.StatusCode, "Failed to update user");
            }

            if (!string.IsNullOrWhiteSpace(dto.Role))
            {
                var assignResult = await AssignRoleToUserAsync(new AssignRolesDto(dto.UserId, dto.Role));
                if (assignResult.StatusCode != 200)
                {
                    return new ApiResponse<string>(assignResult.StatusCode,
                        $"User updated, but role assignment failed: {assignResult.Message}");
                }
            }

            if (!string.IsNullOrWhiteSpace(updatedImageUrl))
            {
                await helper.DeleteUserImageIfExistsAsync(oldImageUrl);
            }

            var machineAssignResult = await addUserMachines(dto.MachineIds, dto.UserId);
            if (machineAssignResult.StatusCode != 200)
                return machineAssignResult;

            return new ApiResponse<string>(200, "User updated successfully");
        }
        catch (Exception ex)
        {
            return new ApiResponse<string>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred: {ex.Message}");
        }
    }


    public async Task<ApiResponse<string>> UpdatePasswordAsync(Guid userId, string oldPassword, string newPassword)
    {
        try
        {
            if (userContext.UserId != userId)
            {
                return new ApiResponse<string>(
                    StatusCodes.Status403Forbidden,
                    "You can only update your own password.");
            }

            if (string.IsNullOrWhiteSpace(oldPassword) || string.IsNullOrWhiteSpace(newPassword))
            {
                return new ApiResponse<string>(
                    StatusCodes.Status400BadRequest,
                    "Old and new passwords cannot be empty.");
            }

            var adminToken = await helper.GetAdminTokenAsync();

            // Get user details to retrieve username
            var userRequest = new HttpRequestMessage(HttpMethod.Get, $"{_keyCloakConfig.UsersEndpoint}/{userId}");
            userRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
            var userResponse = await httpClient.SendAsync(userRequest);
            if (!userResponse.IsSuccessStatusCode)
            {
                var errorMessage = await helper.ExtractErrorMessageAsync(userResponse);
                return new ApiResponse<string>((int)userResponse.StatusCode, $"Failed to fetch user: {errorMessage}");
            }

            var userContent = await userResponse.Content.ReadAsStringAsync();
            var userJson = JsonSerializer.Deserialize<JsonElement>(userContent);
            if (!userJson.TryGetProperty("username", out var usernameProp) || usernameProp.ValueKind == JsonValueKind.Null)
            {
                return new ApiResponse<string>(StatusCodes.Status404NotFound, "User username not found.");
            }
            var username = usernameProp.GetString();

            // Verify old password by attempting login
            var content = new FormUrlEncodedContent(new[]
            {
            new KeyValuePair<string, string>("client_id", _keyCloakConfig.ClientId),
            new KeyValuePair<string, string>("grant_type", Constant.password_grant_type),
            new KeyValuePair<string, string>("username", username),
            new KeyValuePair<string, string>("password", oldPassword),
            new KeyValuePair<string, string>("scope", Constant.scope)
        });

            var response = await httpClient.PostAsync(_keyCloakConfig.TokenEndpoint, content);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                string errorMessage;
                try
                {
                    var errorJson = JsonSerializer.Deserialize<JsonElement>(errorBody);
                    errorMessage = errorJson.TryGetProperty("error_description", out var msg)
                        ? msg.GetString()! : errorBody;
                }
                catch
                {
                    errorMessage = errorBody;
                }

                return new ApiResponse<string>(
                    StatusCodes.Status400BadRequest,
                    "Old password is incorrect."
                );
            }

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<JsonElement>(json);

            var accessToken = data.GetProperty("access_token").GetString()!;
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(accessToken);
            var emailVerified = token.Claims.FirstOrDefault(c => c.Type == "email_verified")?.Value;

            if (emailVerified != "true")
            {
                return new ApiResponse<string>(
                    StatusCodes.Status403Forbidden,
                    "Email not verified. Please verify your email before updating password."
                );
            }

            // If verification succeeds, reset password using admin token
            return await ResetPasswordAsync(userId, newPassword);
        }
        catch (Exception ex)
        {
            return new ApiResponse<string>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred: {ex.Message}");
        }
    }

    public async Task<ApiResponse<string>> AssignRoleToUserAsync(AssignRolesDto dto)
    {
        try
        {

            if (string.IsNullOrWhiteSpace(dto.Role))
                return new ApiResponse<string>(400, "Role cannot be null or empty.");

            var adminToken = await helper.GetAdminTokenAsync();

            var userResult = await GetUserByIdAsync(dto.UserId);
            if (userResult.StatusCode != 200 || userResult.Data == null)
                return new ApiResponse<string>(userResult.StatusCode, $"Failed to fetch user: {userResult.Message}");

            var currentRole = userResult.Data.Role;

            if (string.Equals(currentRole, dto.Role, StringComparison.OrdinalIgnoreCase))
                return new ApiResponse<string>(200, "Already Same Role.");


            var protectionCheck = helper.ValidateRoleProtection(currentRole, dto.Role);
            if (protectionCheck != null)
                return protectionCheck;

            var allRoles = await GetAllRealmRolesJsonAsync(adminToken);
            if (allRoles == null)
                return new ApiResponse<string>(500, "Failed to fetch realm roles.");

            await RemoveCurrentRoleAsync(dto.UserId, currentRole, allRoles, adminToken);
            var assignResult = await AssignNewRoleAsync(dto.UserId, dto.Role, currentRole, allRoles, adminToken);

            return assignResult ?? new ApiResponse<string>(200, "Role assignment updated successfully.");
        }
        catch (Exception ex)
        {
            return new ApiResponse<string>(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
        }
    }

    public async Task<ApiResponse<string>> VerifyEmailAsync(Guid userId)
    {
        try
        {
            var adminToken = await helper.GetAdminTokenAsync();

            var payload = new[] { "VERIFY_EMAIL" };

            var request = new HttpRequestMessage(HttpMethod.Put,
                $"{_keyCloakConfig.ExecuteActionsEndpoint}/{userId}/execute-actions-email")
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await helper.ExtractErrorMessageAsync(response);
                return new ApiResponse<string>((int)response.StatusCode, $"Failed to send verification email: {errorMessage}");
            }

            return new ApiResponse<string>(StatusCodes.Status200OK, "Verification email sent successfully.");
        }
        catch (Exception ex)
        {
            return new ApiResponse<string>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred: {ex.Message}");
        }
    }
    public async Task<ApiResponse<string>> DeleteUserAsync(Guid id)
    {
        try
        {
            var adminToken = await helper.GetAdminTokenAsync();
            var userResponse = await GetUserByIdAsync(id);

            if (userResponse.StatusCode != 200 || userResponse.Data == null)
            {
                return new ApiResponse<string>(
                    StatusCodes.Status404NotFound,
                    $"User not found or could not retrieve user data: {userResponse.Message}");
            }

            var user = userResponse.Data;

            if (helper.IsProtectedUser(user.Role))
            {
                return new ApiResponse<string>(
                    StatusCodes.Status400BadRequest,
                    "Cannot delete 'admin' or protected users.");
            }
            await helper.DeleteUserImageIfExistsAsync(user.ProfileImage);

            var request = new HttpRequestMessage(HttpMethod.Delete, $"{_keyCloakConfig.UsersEndpoint}/{id}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await helper.ExtractErrorMessageAsync(response);
                return new ApiResponse<string>((int)response.StatusCode, errorMessage);
            }

            return new ApiResponse<string>(200, "User deleted successfully.");
        }
        catch (Exception ex)
        {
            return new ApiResponse<string>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred: {ex.Message}");
        }
    }

    public AddUserDto ConvertToAddUserDto(SignUpUserDto signUpUserDto)
    {
        try
        {
            return mapper.Map<AddUserDto>(signUpUserDto);
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<ApiResponse<IEnumerable<string>>> GetAccessibleCustomerIdsAsync(Guid userId)
    {
        var userResult = await GetUserByIdAsync(userId);
        if (userResult.StatusCode != 200 || userResult.Data == null)
            return new ApiResponse<IEnumerable<string>>(userResult.StatusCode, $"Failed to fetch user: {userResult.Message}");

        var user = userResult.Data;

        if (user.Role == ApplicationRoles.RoleSystemAdmin)
        {
            var customerIdsRes = await helper.GetAllCustomerIdsAsync();
            if (customerIdsRes.StatusCode != 200 || customerIdsRes.Data == null)
                return new ApiResponse<IEnumerable<string>>(customerIdsRes.StatusCode, $"Failed to fetch Customer Ids for admin : {customerIdsRes.Message}");
            return new ApiResponse<IEnumerable<string>>(StatusCodes.Status200OK, "Fetched Customers for this User Id", customerIdsRes.Data);
        }

        return new ApiResponse<IEnumerable<string>>(StatusCodes.Status200OK, "Fetched Customers for this User Id", user.CustomerIds ?? []);
    }

    public async Task<ApiResponse<CustomerUsersSummaryDto>> GetCustomerUsersSummary(Guid customerId)
    {
        try
        {
            await CustomerAccessHelper.ValidateCustomerAccessAsync(userContext, customerId);

            var result = await unitOfWork.CustomerRepository.GetAsync(customerId);
            if (result.IsLeft)
            {
                return new ApiResponse<CustomerUsersSummaryDto>(
                    StatusCodes.Status404NotFound,
                    $"Customer with ID {customerId} not found.");
            }

            var customer = result.IfRight();
            var customerDto = mapper.Map<CustomerDto>(customer!);

            var userQuery = new UserQueryParameters { CustomerId = customerId.ToString(), Top = int.MaxValue };
            var getAllUsersResponse = await GetAllUsersAsync(userQuery, false);

            if (getAllUsersResponse.StatusCode != 200 || getAllUsersResponse.Data == null)
            {
                return new ApiResponse<CustomerUsersSummaryDto>(
                    getAllUsersResponse.StatusCode,
                    $"Failed to fetch users for customer {customerId}: {getAllUsersResponse.Message}");
            }

            var userList = getAllUsersResponse.Data;

            var summary = helper.BuildCustomerUsersSummary(customerDto, userList);

            return new ApiResponse<CustomerUsersSummaryDto>(
                StatusCodes.Status200OK,
                "Customer users summary retrieved",
                summary);
        }
        catch (UnauthorizedAccessException ex)
        {
            return new ApiResponse<CustomerUsersSummaryDto>(
                StatusCodes.Status403Forbidden,
                ex.Message);
        }
        catch (Exception ex)
        {
            return new ApiResponse<CustomerUsersSummaryDto>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred while fetching user summary: {ex.Message}");
        }
    }


    #region Private Helper Methods
    private async Task<string> UploadProfileImageAsync(string? profileImage)
    {
        if (String.IsNullOrWhiteSpace(profileImage)) return string.Empty;

        var uploadedUri = await blobStorageService.UploadBase64Async(profileImage, BlobStorageConstants.UserFolder);
        return uploadedUri.AbsoluteUri;
    }
    private async Task<HttpResponseMessage> SendCreateUserRequestAsync(object userPayload, string adminToken)
    {
        var json = JsonSerializer.Serialize(userPayload);
        var request = new HttpRequestMessage(HttpMethod.Post, _keyCloakConfig.UsersEndpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        return await httpClient.SendAsync(request);
    }
    private async Task<ApiResponse<string>> UpdateUserStatusAsync(Guid userId, bool enabled, string action)
    {
        try
        {
            var adminToken = await helper.GetAdminTokenAsync();
            var payload = new { enabled };

            var request = new HttpRequestMessage(HttpMethod.Put, $"{_keyCloakConfig.UsersEndpoint}/{userId}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
            request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await helper.ExtractErrorMessageAsync(response);
                return new ApiResponse<string>((int)response.StatusCode, $"Failed to {action} user: {errorMessage}");
            }

            return new ApiResponse<string>(200, $"User account {action}d successfully.");
        }
        catch (Exception ex)
        {
            return new ApiResponse<string>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred: {ex.Message}");
        }
    }
    private async Task<HttpResponseMessage> GetCurrentUserDataAsync(Guid id, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"{_keyCloakConfig.UsersEndpoint}/{id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await httpClient.SendAsync(request);
    }
    private async Task<HttpResponseMessage> UpdateUserDataAsync(Guid id, Dictionary<string, object?> updatedUser, string token)
    {
        var json = JsonSerializer.Serialize(updatedUser, new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });

        var request = new HttpRequestMessage(HttpMethod.Put, $"{_keyCloakConfig.UsersEndpoint}/{id}")
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await httpClient.SendAsync(request);
    }
    private async Task<List<JsonElement>?> GetAllRealmRolesJsonAsync(string adminToken)
    {
        var rolesRequest = new HttpRequestMessage(HttpMethod.Get, _keyCloakConfig.RolesEndpoint);
        rolesRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var rolesResponse = await httpClient.SendAsync(rolesRequest);
        if (!rolesResponse.IsSuccessStatusCode) return null;

        var rolesJson = JsonDocument.Parse(await rolesResponse.Content.ReadAsStringAsync()).RootElement;
        return rolesJson.EnumerateArray().ToList();
    }
    private async Task RemoveCurrentRoleAsync(Guid userId, string? currentRole, List<JsonElement> allRoles, string adminToken)
    {
        if (string.IsNullOrWhiteSpace(currentRole) ||
            string.Equals(currentRole, Constant.keyCloakDefaultRole, StringComparison.OrdinalIgnoreCase))
            return;

        var roleToRemove = allRoles.FirstOrDefault(r =>
            r.TryGetProperty("name", out var name) &&
            string.Equals(name.GetString(), currentRole, StringComparison.OrdinalIgnoreCase));

        if (roleToRemove.ValueKind != JsonValueKind.Undefined)
        {
            var removeRequest = new HttpRequestMessage(HttpMethod.Delete,
                $"{_keyCloakConfig.UsersEndpoint}/{userId}/role-mappings/realm")
            {
                Content = new StringContent(JsonSerializer.Serialize(new[] { roleToRemove }), Encoding.UTF8, "application/json"),
                Headers = { Authorization = new AuthenticationHeaderValue("Bearer", adminToken) }
            };

            await httpClient.SendAsync(removeRequest);
        }
    }
    private async Task<ApiResponse<string>?> AssignNewRoleAsync(Guid userId, string newRole, string? currentRole, List<JsonElement> allRoles, string adminToken)
    {
        if (string.Equals(currentRole, newRole, StringComparison.OrdinalIgnoreCase))
            return null;

        var roleToAssign = allRoles.FirstOrDefault(r =>
            r.TryGetProperty("name", out var name) &&
            string.Equals(name.GetString(), newRole, StringComparison.OrdinalIgnoreCase));

        if (roleToAssign.ValueKind == JsonValueKind.Undefined)
            return new ApiResponse<string>(400, $"The role '{newRole}' does not exist in Keycloak.");

        var assignRequest = new HttpRequestMessage(HttpMethod.Post,
            $"{_keyCloakConfig.UsersEndpoint}/{userId}/role-mappings/realm")
        {
            Content = new StringContent(JsonSerializer.Serialize(new[] { roleToAssign }), Encoding.UTF8, "application/json"),
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", adminToken) }
        };

        var assignResponse = await httpClient.SendAsync(assignRequest);
        if (!assignResponse.IsSuccessStatusCode)
            return new ApiResponse<string>((int)assignResponse.StatusCode, "Failed to assign new role.");

        return null;
    }


    private async Task<ApiResponse<string>> addUserMachines(List<Guid>? machineIds, Guid userId)
    {
        if (machineIds == null || !machineIds.Any())
            return new ApiResponse<string>(200, "Machine IDs null or empty.");

        // Remove existing assignments
        var userMachineFilters = new List<Expression<Func<UserMachine, bool>>>
        {
            um => um.UserId == userId
        };

        var existingAssignments = await userMachineRepository.GetListAsync(
            null,
            pageFilterExpression: um => true, // base expression
            documentFilterExpression: userMachineFilters,
            order: q => q.OrderBy(um => um.MachineId)
        );

        await userMachineRepository.DeleteRangeAsync(existingAssignments); // ✅ uses repository

        // ✅ Add new assignments
        foreach (var machineId in machineIds)
        {
            await userMachineRepository.AddAsync(new UserMachine
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                MachineId = machineId
            });
        }
        await unitOfWork.SaveChangesAsync();


        return new ApiResponse<string>(200, "Machines assigned successfully");
    }


    private async Task<ApiResponse<string>> validateAddUserMachines(List<Guid>? machineIds)
    {
        if (machineIds == null || !machineIds.Any())
            return new ApiResponse<string>(200, "Machine IDs null or empty.");
        var machineFilters = new List<Expression<Func<Machine, bool>>>
        {
            m => machineIds.Contains(m.Id)
        };

        var machines = await machineRepository.GetListAsync(
            null,
            m => true,
            machineFilters,
            q => q.OrderBy(m => m.Id)
        );


        if (machines.Count != machineIds.Count)
            return new ApiResponse<string>(400, " One or more machines are invalid.");


        return new ApiResponse<string>(200, "Machines Id are valid");
    }




    #endregion
}