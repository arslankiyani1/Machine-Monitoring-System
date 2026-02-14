namespace MMS.Adapters.Keycloak.Repositories;

public class UserServiceHelper(
        HttpClient _httpClient,
        IBlobStorageService _blobStorageService,
        ICustomerRepository _customerRepository,
        IUserContextService userContext,
        IUnitOfWork unitOfWork,
        IOptions<KeycloakSettings> options,
        IServiceProvider serviceProvider,
        IUserContextService userContextService,
AutoMapperResult mapper
        )
{
    private readonly KeycloakSettings _keyCloakConfig = options.Value;
    public async Task<string> GetAdminTokenAsync()
    {
        var tokenRequest = new HttpRequestMessage(HttpMethod.Post, _keyCloakConfig.TokenEndpoint);
        var form = new Dictionary<string, string>
        {
            ["client_id"] = _keyCloakConfig.ClientId,
            ["grant_type"] = Constant.password_grant_type,
            ["username"] = _keyCloakConfig.AdminUserName,
            ["password"] = _keyCloakConfig.AdminPassword,
        };

        tokenRequest.Content = new FormUrlEncodedContent(form);
        var result = await _httpClient.SendAsync(tokenRequest);
        result.EnsureSuccessStatusCode();

        var json = await result.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<JsonElement>(json);
        return data.GetProperty("access_token").GetString()!;
    }

    public async Task<bool> SendVerificationEmailAsync(HttpResponseMessage response, bool isSignup, string adminToken)
    {
        if (!isSignup)
        {
            return true; // Skip email verification for non-signup user creation
        }

        var location = response.Headers.Location?.ToString();
        if (string.IsNullOrEmpty(location))
        {
            return false;
        }

        var userIdStr = location.Split('/').Last();
        if (!Guid.TryParse(userIdStr, out var userId))
        {
            return false;
        }

        var request = new HttpRequestMessage(HttpMethod.Post, $"{_keyCloakConfig.UsersEndpoint}/{userId}/send-verify-email");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var sendEmailResponse = await _httpClient.SendAsync(request);

        return sendEmailResponse.IsSuccessStatusCode;
    }

    public async Task<List<RoleDto>> GetUserRolesAsync(Guid userId, string adminToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get,
            $"{_keyCloakConfig.UsersEndpoint}/{userId}/role-mappings/realm");

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
            return new List<RoleDto>();

        var json = await response.Content.ReadAsStringAsync();
        var rolesJson = JsonSerializer.Deserialize<JsonElement>(json);

        var roles = new List<RoleDto>();

        foreach (var role in rolesJson.EnumerateArray())
        {
            roles.Add(new RoleDto(
                role.GetProperty("id").GetString()!,
                role.GetProperty("name").GetString()!,
                role.TryGetProperty("description", out var desc) ? desc.GetString() : null
            ));
        }

        return roles;
    }

    public async Task<ApiResponse<string>> ValidateRealmRoleAsync(string? role, Func<Task<ApiResponse<List<RoleDto>>>> getAllRealmRolesFunc)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            return new ApiResponse<string>(200, "No role provided."); // Valid by default if role is null/empty
        }

        var realmRolesResponse = await getAllRealmRolesFunc();

        if (realmRolesResponse.StatusCode != 200 || realmRolesResponse.Data == null)
        {
            return new ApiResponse<string>(500, "Failed to fetch realm roles.");
        }

        var validRoles = realmRolesResponse.Data
            .Select(r => r.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (!validRoles.Contains(role))
        {
            return new ApiResponse<string>(400, $"Role '{role}' is not a valid realm role.");
        }

        return new ApiResponse<string>(200, "Role is valid.");
    }

    public async Task<ApiResponse<string>?> DeleteUserImageIfExistsAsync(string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            return null;

        try
        {
            await _blobStorageService.DeleteBase64Async(imageUrl);
            return null;
        }
        catch (Exception ex)
        {
            return new ApiResponse<string>(
                StatusCodes.Status500InternalServerError,
                $"Failed to delete user's image from blob storage: {ex.Message}");
        }
    }

    public static UserModel MapToUserDto(Guid userId, JsonElement user, List<RoleDto>? roles, IEnumerable<MachineDto>? machines)
    {
        var attributes = user.GetProperty("attributes");

        var appRole = roles?.FirstOrDefault(r => !string.Equals(r.Name, Constant.keyCloakDefaultRole, StringComparison.OrdinalIgnoreCase));

        return new UserModel
        {
            UserId = userId,
            Username = user.GetProperty("username").GetString()!,
            Email = user.TryGetProperty("email", out var email) ? email.GetString() : null,
            Enabled = user.GetProperty("enabled").GetBoolean(),
            EmailVerified = user.GetProperty("emailVerified").GetBoolean(),
            FirstName = user.TryGetProperty("firstName", out var fn) ? fn.GetString() : null,
            LastName = user.TryGetProperty("lastName", out var ln) ? ln.GetString() : null,
            JobTitle = JsonAttributeUtils.GetAttr(attributes, "jobTitle"),
            Department = JsonAttributeUtils.GetAttr(attributes, "department"),
            CustomerIds = JsonAttributeUtils.GetAttrList(attributes, "customerId"),
            ProfileImage = JsonAttributeUtils.GetAttr(attributes, "profileImageUrl"),
            FcmTokens = JsonAttributeUtils.GetAttrList(attributes, "fcmTokens"),
            PhoneCode = JsonAttributeUtils.GetAttr(attributes, "phoneCode"),
            PhoneNumber = JsonAttributeUtils.GetAttr(attributes, "phoneNumber"),
            City = JsonAttributeUtils.GetAttr(attributes, "city"),
            Country = JsonAttributeUtils.GetAttr(attributes, "country"),
            Region = JsonAttributeUtils.GetAttr(attributes, "region"),
            State = JsonAttributeUtils.GetAttr(attributes, "state"),
            TimeZone = JsonAttributeUtils.GetAttr(attributes, "timeZone"),
            Language = JsonAttributeUtils.GetLanguageAttr(attributes, "language"),
            Role = appRole?.Name,
            RoleDisplayName = appRole != null ? RoleTranslator.GetTranslatedRole(appRole.Name, JsonAttributeUtils.GetLanguageAttr(attributes, "language")) : null,
            Machines = machines
        };
    }
  
    public static Dictionary<string, object?> BuildUpdatedUserPayload(JsonElement currentJson, UpdateUserDto dto, string? updatedImageUrl)
    {
       

        var updatedUser = new Dictionary<string, object?>
        {
            ["firstName"] = string.IsNullOrWhiteSpace(dto.FirstName)
                ? (currentJson.TryGetProperty("firstName", out var firstNameProp) &&
                   firstNameProp.ValueKind != JsonValueKind.Null &&
                   firstNameProp.ValueKind != JsonValueKind.Undefined
                    ? firstNameProp.GetString()
                    : null)
                : dto.FirstName,

            ["lastName"] = string.IsNullOrWhiteSpace(dto.LastName)
            ? (currentJson.TryGetProperty("lastName", out var lastNameProp) &&
               lastNameProp.ValueKind != JsonValueKind.Null &&
               lastNameProp.ValueKind != JsonValueKind.Undefined
                ? lastNameProp.GetString()
                : null)
            : dto.LastName,

            ["email"] = currentJson.TryGetProperty("email", out var emailVal)
                ? emailVal.GetString()
                : null,
            //["enabled"] =  currentJson.GetProperty("enabled").GetBoolean(),

            ["enabled"] = dto.Enabled!=null ? dto.Enabled :currentJson.GetProperty("enabled").GetBoolean(),
            ["emailVerified"] = currentJson.GetProperty("emailVerified").GetBoolean(),
            ["username"] = currentJson.GetProperty("username").GetString()
        };

        // Merge attributes
        var mergedAttributes = new Dictionary<string, string[]>();

        if (currentJson.TryGetProperty("attributes", out var attrs))
        {
            foreach (var prop in attrs.EnumerateObject())
                mergedAttributes[prop.Name] = prop.Value.EnumerateArray().Select(x => x.GetString() ?? "").ToArray();
        }

        // Helper local method to assign string properties
        void SetAttr(string key, string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
                mergedAttributes[key] = new[] { value };
        }

    
        void SetAttrEmptyTo(string key, string? value)
        {
            if (value is null)
                return;

            mergedAttributes[key] = new[] { value == string.Empty ? "" : value };
        }

        SetAttr("jobTitle", dto.JobTitle);
        SetAttr("department", dto.Department);
        SetAttr("phoneCode", dto.PhoneCode);
        SetAttr("phoneNumber", dto.PhoneNumber);
        SetAttrEmptyTo("city", dto.City);
        SetAttrEmptyTo("country", dto.Country);
        SetAttrEmptyTo("region", dto.Region);
        SetAttrEmptyTo("state", dto.State);
        SetAttr("timeZone", dto.TimeZone);
        SetAttr("language", dto.Language?.ToString());

        if (dto.CustomerIds is { Count: > 0 })
            mergedAttributes["customerId"] = dto.CustomerIds.Select(id => id.ToString()).ToArray();

        // Preserve old image if updated one is not given
        if (!string.IsNullOrWhiteSpace(updatedImageUrl))
        {
            mergedAttributes["profileImageUrl"] = [updatedImageUrl];
        }
        else if (mergedAttributes.TryGetValue("profileImageUrl", out var existingImages))
        {
            mergedAttributes["profileImageUrl"] = existingImages;
        }
        mergedAttributes["fcmTokens"] = (dto.FcmTokens != null && dto.FcmTokens.Any())
            ? BuildFcmTokensArray(dto.FcmTokens)
            : (attrs.TryGetProperty("fcmTokens", out var fcmVal)
                ? fcmVal.EnumerateArray().Select(x => x.GetString() ?? "").ToArray()
                : Array.Empty<string>());

        updatedUser["attributes"] = mergedAttributes;
        return updatedUser;
    }

    // Helper method to build deduplicated FCM tokens array
    private static string[] BuildFcmTokensArray(List<string> fcmTokens)
    {
        var fcmMap = new Dictionary<string, string>();
        foreach (var fcm in fcmTokens)
        {
            var parts = fcm.Split(new string[] { "||" }, 2, StringSplitOptions.None);
            if (parts.Length >= 1 && !string.IsNullOrWhiteSpace(parts[0]))
            {
                var deviceId = parts[0];
                var token = parts.Length > 1 ? parts[1] : "";
                fcmMap[deviceId] = token; // Last token for each deviceId wins
            }
        }
        return fcmMap.Count > 0 ? fcmMap.Select(kv => $"{kv.Key}||{kv.Value}").ToArray() : Array.Empty<string>();
    }

    public static string GetOldImageUrl(JsonElement userJson)
    {
        if (userJson.TryGetProperty("attributes", out var attrs)
            && attrs.TryGetProperty("profileImageUrl", out var imagesElement)
            && imagesElement.ValueKind == JsonValueKind.Array)
        {
            return imagesElement.EnumerateArray().FirstOrDefault().GetString() ?? string.Empty;
        }

        return string.Empty;
    }

    public async Task<ApiResponse<string>> CheckCustomerIdsExistAsync(List<Guid> customerIds)
    {
        try
        {
            if (customerIds == null || customerIds.Count == 0)
            {
                return new ApiResponse<string>(
                    StatusCodes.Status200OK,
                    "Customer ID not provided to update."
                );
            }

            var invalidIds = new List<Guid>();

            foreach (var id in customerIds)
            {
                var result = await _customerRepository.GetAsync(id);
                if (result.IsLeft)
                {
                    invalidIds.Add(id);
                }
            }

            if (invalidIds.Count == 0)
            {
                return new ApiResponse<string>(
                    StatusCodes.Status200OK,
                    "All customer IDs are valid."
                );
            }

            return new ApiResponse<string>(
                StatusCodes.Status400BadRequest,
                $"Invalid customer ID(s): {string.Join(", ", invalidIds)}"
            );
        }
        catch (Exception ex)
        {
            return new ApiResponse<string>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred while checking customer IDs. {ex.Message}"
            );
        }
    }

    public async Task<ApiResponse<IEnumerable<string>>> GetAllCustomerIdsAsync()
    {
        try
        {
            // No filtering, no pagination – get all customers
            var customers = await _customerRepository.GetListAsync(
                pageParameters: null,
                pageParametersExpression: c => true,
                documentFilterExpression: null,
                order: q => q.OrderBy(c => c.Name)
            );

            var customerIds = customers.Select(c => c.Id.ToString());

            return new ApiResponse<IEnumerable<string>>(
                StatusCodes.Status200OK,
                $"{nameof(Customer)} IDs {ResponseMessages.GetAll}",
                customerIds
            );
        }
        catch (Exception ex)
        {
            return new ApiResponse<IEnumerable<string>>(
                StatusCodes.Status500InternalServerError,
                "An error occurred while retrieving customer IDs. " + ex.Message
            );
        }
    }

    public async Task<ApiResponse<string>> AssignRoleIfApplicableAsync(
        HttpResponseMessage response,
        AddUserDto dto,
        bool isSignup,
        Func<AssignRolesDto, Task<ApiResponse<string>>> assignRoleFunc)
    {
        if (isSignup || string.IsNullOrWhiteSpace(dto.Role))
        {
            return new ApiResponse<string>(200, "Role assignment skipped.");
        }

        var location = response.Headers.Location?.ToString();
        if (string.IsNullOrEmpty(location))
        {
            return new ApiResponse<string>(500, "User created but no location returned.");
        }

        var userIdStr = location.Split('/').Last();
        if (!Guid.TryParse(userIdStr, out var userId))
        {
            return new ApiResponse<string>(500, "Failed to parse user ID from Keycloak response.");
        }

        var assignResult = await assignRoleFunc(new AssignRolesDto(userId, dto.Role));
        if (assignResult.StatusCode != 200)
        {
            return new ApiResponse<string>(
                assignResult.StatusCode,
                $"User created, but role assignment failed: {assignResult.Message}"
            );
        }

        return new ApiResponse<string>(200, "Role assigned successfully.");
    }

    public Task<ApiResponse<string>> GetUserIdFromResponse(HttpResponseMessage response)
    {
        var location = response.Headers.Location?.ToString();
        if (string.IsNullOrEmpty(location))
        {
            return Task.FromResult(new ApiResponse<string>(500, "User created but no location returned."));
        }

        var userIdStr = location.Split('/').Last();
        if (!Guid.TryParse(userIdStr, out _))
        {
            return Task.FromResult(new ApiResponse<string>(500, "Failed to parse user ID from Keycloak response."));
        }

        return Task.FromResult(new ApiResponse<string>(200, "User ID fetched successfully.", userIdStr));
    }

    public  async Task<string> ExtractErrorMessageAsync(HttpResponseMessage response)
    {
        var errorContent = await response.Content.ReadAsStringAsync();
        try
        {
            var errorObj = JsonSerializer.Deserialize<Dictionary<string, string>>(errorContent);
            return errorObj?["error"] ?? errorContent;
        }
        catch
        {
            return errorContent;
        }
    }

    public  async Task<string> ExtractKeycloakErrorAsync(HttpResponseMessage response)
    {
        var errorBody = await response.Content.ReadAsStringAsync();
        try
        {
            var errorJson = JsonSerializer.Deserialize<JsonElement>(errorBody);
            return errorJson.TryGetProperty("errorMessage", out var msg) ? msg.GetString()! : errorBody;
        }
        catch
        {
            return errorBody;
        }
    }

    public  bool IsUserToBeShown(List<RoleDto> roles)
    {
        return roles.Any(r =>
            string.Equals(r.Name, Constant.keyClockAdminRole, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(r.Name, ApplicationRoles.RoleSystemAdmin, StringComparison.OrdinalIgnoreCase) || 
            string.Equals(r.Name, ApplicationRoles.RoleMMSBridge, StringComparison.OrdinalIgnoreCase));
    }

    public  bool IsProtectedUser(string? role)
    {
        return string.Equals(role, Constant.keyClockAdminRole, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(role, ApplicationRoles.RoleSystemAdmin, StringComparison.OrdinalIgnoreCase);
    }

    public bool IsProtectedRole(string role)
    {
        return string.Equals(role, Constant.keyClockAdminRole, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(role, ApplicationRoles.RoleSystemAdmin, StringComparison.OrdinalIgnoreCase);
    }

    public bool IsSystemRole(string? name)
    {
        return string.Equals(name, Constant.keyCloakDefaultRole, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(name, Constant.keyClockAdminRole, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(name, ApplicationRoles.RoleSystemAdmin, StringComparison.OrdinalIgnoreCase);
    }

    public ApiResponse<string>? ValidateRoleProtection(string? currentRole, string newRole)
    {
        if (!string.IsNullOrWhiteSpace(currentRole) && (IsProtectedRole(currentRole)))
        {
            return new ApiResponse<string>(StatusCodes.Status401Unauthorized, "Protected user role cannot be changed");
        }

        if (IsProtectedRole(newRole))
        {
            return new ApiResponse<string>(StatusCodes.Status401Unauthorized, "Protected user role cannot be assigned");
        }

        return null;
    }

    public  List<RoleDto> FilterRealmRoles(JsonElement json, bool isMssAdmin)
    {
        var roles = new List<RoleDto>();

        foreach (var role in json.EnumerateArray())
        {
            var name = role.GetProperty("name").GetString();

            // Always skip these roles
            if (IsSystemRole(name)) continue;

            // Skip customer admin role unless current user is system admin for all - 2/10/25 - Sir Hammad Asked
            if (string.Equals(name, ApplicationRoles.RoleMMSBridge, StringComparison.OrdinalIgnoreCase))// && !isMssAdmin)
                continue;

            roles.Add(new RoleDto(
                role.GetProperty("id").GetString()!,
                name!,
                role.TryGetProperty("description", out var desc) ? desc.GetString() : null));
        }

        return roles;
    }


    public async Task<string> BuildUsersQueryUrlAsync(UserQueryParameters query)
    {
        var qs = new List<string>();

        if (query.Skip is not null) qs.Add($"first={query.Skip}");
        if (query.Top is not null) qs.Add($"max={query.Top}");
        if (!string.IsNullOrWhiteSpace(query.Term)) qs.Add($"search={query.Term}");
        if (!string.IsNullOrWhiteSpace(query.Username)) qs.Add($"username={query.Username}");
        if (!string.IsNullOrWhiteSpace(query.Email)) qs.Add($"email={query.Email}");
        if (query.Enabled is not null) qs.Add($"enabled={query.Enabled.Value.ToString().ToLower()}");
        if (query.EmailVerified is not null) qs.Add($"emailVerified={query.EmailVerified.Value.ToString().ToLower()}");
        if (query.Exact is not null) qs.Add($"exact={query.Exact.Value.ToString().ToLower()}");

        // 🔒 Customer access validation
        if (!string.IsNullOrWhiteSpace(query.CustomerId))
        {
            // Validate single customer access
            await CustomerAccessHelper.ValidateCustomerAccessAsync(userContextService, Guid.Parse(query.CustomerId));
            qs.Add($"q=customerId:{query.CustomerId}");
        }
        else
        {
            var accessibleCustomerIds = CustomerAccessHelper.GetAccessibleCustomerIds(userContextService);
            if (accessibleCustomerIds != null && accessibleCustomerIds.Any())
            {
                // ✅ Only use the first accessible customerId
                var firstCustomerId = accessibleCustomerIds.First();
                qs.Add($"q=customerId:{firstCustomerId}");
            }
        }

        var url = _keyCloakConfig.UsersEndpoint;
        if (qs.Any())
            url += "?" + string.Join("&", qs);

        return url;
    }


    public string BuildCreateUserSuccessMessage(bool isSignup)
    {
        return isSignup
            ? "Signup complete! A verification email has been sent."
            : "User created & enabled.";
    }

  
    public object BuildCreateUserPayload(AddUserDto dto, string profileImageUrl, bool isSignUp)
    {
        return new
        {
            username = dto.Username,
            email = dto.Email,
            firstName = dto.FirstName,
            lastName = dto.LastName,
            enabled = true, // remaining will do when signup completes
            emailVerified = isSignUp ? false : true,
            credentials = new[]
            {
            new { type = Constant.password_grant_type, value = dto.Password, temporary = false }
        },
            attributes = new
            {
                jobTitle = dto.JobTitle,
                department = dto.Department,
                customerId = dto.CustomerIds,
                profileImageUrl = profileImageUrl,
                fcmTokens = dto.FcmTokens?.ToArray(),
                phoneCode = dto.PhoneCode,
                phoneNumber = dto.PhoneNumber,
                city = dto.City,
                country = dto.Country,
                region = dto.Region,
                state = dto.State,
                timeZone = dto.TimeZone,
                language = dto.Language?.ToString()
            }
        };
    }



    public async Task<IEnumerable<MachineDto>> GetUserMachinesAsync(Guid userId)
    {
        // Get all UserMachine mappings for the given user
        var userMachineFilters = new List<Expression<Func<UserMachine, bool>>>
    {
        um => um.UserId == userId
    };

        var userMachineMappings = await unitOfWork.UserMachineRepository.GetListAsync(
            null,
            pageFilterExpression: um => true,
            documentFilterExpression: userMachineFilters,
            order: q => q.OrderBy(um => um.MachineId)
        );

        if (userMachineMappings == null || !userMachineMappings.Any())
            return Enumerable.Empty<MachineDto>();

        // Extract machine IDs
        var machineIds = userMachineMappings.Select(um => um.MachineId).ToList();

        // Query machines by IDs
        var machineFilters = new List<Expression<Func<Machine, bool>>>
    {
        m => machineIds.Contains(m.Id)
    };

        var machines = await unitOfWork.MachineRepository.GetListAsync(
            null,
            m => true,
            machineFilters,
            null
        );
        return mapper.Map<IEnumerable<MachineDto>>(machines);

    }


    public CustomerUsersSummaryDto BuildCustomerUsersSummary(CustomerDto customer, List<UserModel> users)
    {
        int totalUsers = users.Count;

        int assigned = users.Count(u => u.Machines != null && u.Machines.Any());
        int unassigned = totalUsers - assigned;
        int enabled = users.Count(u => u.Enabled);
        int disabled = totalUsers - enabled;

        return new CustomerUsersSummaryDto
        {
            Id = customer.Id,
            Name = customer.Name,
            ImageUrl = customer.ImageUrls,
            TotalUsers = totalUsers,
            Assigned = assigned,
            UnAssigned = unassigned,
            Enabled= enabled,
            Disabled= disabled,

        };
    }

    public async Task<List<UserModel>> ProcessUsersResponseAsync(
        JsonElement usersJson,
        string adminToken,
        bool requireDetailedUserModel,bool showAdminsAndCurrentUser)
    {
        var results = new List<UserModel>();

        if (requireDetailedUserModel)
        {
            // Run in parallel because we’re doing async I/O calls (roles, machines)
            var userTasks = usersJson.EnumerateArray()
                .Select(async user =>
                {
                    try
                    {
                        var userIdStr = user.GetProperty("id").GetString();
                        if (string.IsNullOrWhiteSpace(userIdStr)) return null;
                        var userId = Guid.Parse(userIdStr);

                        if (userId == userContext.UserId)
                        {
                            if (!showAdminsAndCurrentUser)
                            {
                                return null;
                            }
                        }

                        using var scope = serviceProvider.CreateScope();
                        var userServiceHelper = scope.ServiceProvider.GetRequiredService<UserServiceHelper>();

                        var roles = await userServiceHelper.GetUserRolesAsync(userId, adminToken);
                        if (!showAdminsAndCurrentUser && IsUserToBeShown(roles))
                        {
                            return null;
                        }

                        var machines = await userServiceHelper.GetUserMachinesAsync(userId);
                        return MapToUserDto(userId, user, roles, machines);
                    }
                    catch
                    {
                        return null;
                    }
                });

            var detailedResults = await Task.WhenAll(userTasks);
            results.AddRange(detailedResults.Where(x => x != null)!);
        }
        else
        {
            // Simple loop – fast and no need for async
            foreach (var user in usersJson.EnumerateArray())
            {
                if (user.GetProperty("id").GetString() == userContext.UserId.ToString())
                    continue;

                try
                {
                    var userIdStr = user.GetProperty("id").GetString();
                    if (string.IsNullOrWhiteSpace(userIdStr)) continue;

                    var userId = Guid.Parse(userIdStr);
                    results.Add(MapToUserDto(userId, user, null, null));
                }
                catch
                {
                    continue;
                }
            }
        }

        return results;
    }

    public string GetBaseUrl()
    {
        if (string.IsNullOrWhiteSpace(_keyCloakConfig.KeyCloakUrl) || string.IsNullOrWhiteSpace(_keyCloakConfig.Realm))
        {
            throw new InvalidOperationException("Keycloak URL or Realm is not configured.");
        }

        return $"{_keyCloakConfig.KeyCloakUrl.TrimEnd('/')}/admin/realms/{_keyCloakConfig.Realm}";
    }
}