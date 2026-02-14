namespace MMS.Adapters.Keycloak.Extensions;

public static class KeycloakExtensions
{
    public static void AddKeyCloakServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<KeycloakSettings>(configuration.GetSection("Keycloak"));
        services.AddScoped<UserServiceHelper>();
        services.AddHttpClient<IAuthService, AuthService>();
        services.AddHttpClient<IUserService, UserService>();
        services.AddHttpContextAccessor();
        services.AddScoped<IUserContextService, UserContextService>();

    }

    public static IServiceCollection AddKeycloakAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var keycloakSettings = new KeycloakSettings();
        configuration.GetSection("Keycloak").Bind(keycloakSettings);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = keycloakSettings.Authority;
                options.Audience = keycloakSettings.ClientId;
                options.RequireHttpsMetadata = false; // Set to true in production

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true, // verify signature
                    ValidateAudience = false,
                    ValidateLifetime = true, // Validate token expiration
                    ClockSkew = TimeSpan.Zero, //  strict expiration time
                    RoleClaimType = ClaimTypes.Role,
                    NameClaimType = "preferred_username",
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        var identity = context.Principal?.Identity as ClaimsIdentity;

                        var realmAccessClaim = context.Principal?.FindFirst("realm_access")?.Value;
                        if (!string.IsNullOrWhiteSpace(realmAccessClaim))
                        {
                            using var document = JsonDocument.Parse(realmAccessClaim);
                            if (document.RootElement.TryGetProperty("roles", out var roles))
                            {
                                foreach (var role in roles.EnumerateArray())
                                {
                                    var roleName = role.GetString();
                                    if (!string.IsNullOrWhiteSpace(roleName) && roleName != Constant.keyCloakDefaultRole)
                                    {
                                        identity?.AddClaim(new Claim(ClaimTypes.Role, roleName));
                                    }
                                }
                            }
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        return services;
    }

    public static IServiceCollection AddKeycloakAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {

            options.AddPolicy(AuthorizationPolicies.RequireSystemAdmin, policy =>
                policy.RequireRole(ApplicationRoles.RoleSystemAdmin));

            options.AddPolicy(AuthorizationPolicies.RequireCustomerAdmin, policy =>
                policy.RequireRole(ApplicationRoles.RoleCustomerAdmin));

            options.AddPolicy(AuthorizationPolicies.RequireTechnicianOrOperator, policy =>
                policy.RequireRole(ApplicationRoles.RoleTechnician, ApplicationRoles.RoleOperator));

            options.AddPolicy(AuthorizationPolicies.RequireViewer, policy =>
                policy.RequireRole(ApplicationRoles.RoleViewer));

            options.AddPolicy(AuthorizationPolicies.RequireTechnician, policy =>
                policy.RequireRole(ApplicationRoles.RoleTechnician));

            options.AddPolicy(AuthorizationPolicies.RequireOperator, policy =>
                policy.RequireRole(ApplicationRoles.RoleOperator));


            options.AddPolicy(AuthorizationPolicies.RequireMMSBridge, policy =>
                policy.RequireRole(ApplicationRoles.RoleMMSBridge));

            options.AddPolicy(AuthorizationPolicies.RequireSysAdminOrCustAdmin, policy =>
               policy.RequireRole(ApplicationRoles.RoleSystemAdmin, ApplicationRoles.RoleCustomerAdmin));

            options.AddPolicy(AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewer, policy =>
               policy.RequireRole(ApplicationRoles.RoleSystemAdmin, ApplicationRoles.RoleCustomerAdmin, ApplicationRoles.RoleViewer));

            options.AddPolicy(AuthorizationPolicies.RequireSysAdminOrViewer, policy =>
               policy.RequireRole(ApplicationRoles.RoleSystemAdmin, ApplicationRoles.RoleViewer));

            options.AddPolicy(AuthorizationPolicies.RequireTechnicianOrOperator, policy =>
               policy.RequireRole(ApplicationRoles.RoleTechnician, ApplicationRoles.RoleOperator));

            options.AddPolicy(AuthorizationPolicies.RequireSysAdminOrCustAdminOrTechnician, policy =>
              policy.RequireRole(ApplicationRoles.RoleSystemAdmin, ApplicationRoles.RoleCustomerAdmin, ApplicationRoles.RoleTechnician));

            options.AddPolicy(AuthorizationPolicies.RequireSysAdminOrCustAdminOrOperator, policy =>
            policy.RequireRole(ApplicationRoles.RoleSystemAdmin, ApplicationRoles.RoleCustomerAdmin, ApplicationRoles.RoleOperator));

            options.AddPolicy(AuthorizationPolicies.RequireSysAdminOrCustAdminOrTechnicianOrOperator, policy =>
              policy.RequireRole(ApplicationRoles.RoleSystemAdmin, ApplicationRoles.RoleCustomerAdmin, ApplicationRoles.RoleTechnician, ApplicationRoles.RoleOperator));

            options.AddPolicy(AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewerOrTechnicianOrOperator, policy =>
                policy.RequireRole(ApplicationRoles.RoleSystemAdmin, ApplicationRoles.RoleCustomerAdmin, ApplicationRoles.RoleTechnician, ApplicationRoles.RoleOperator, ApplicationRoles.RoleViewer));

            options.AddPolicy(AuthorizationPolicies.RequireSysAdminOrCustAdminOrViewerOrTechnicianOrOperatorOrMMSBridge, policy =>
                    policy.RequireRole(ApplicationRoles.RoleSystemAdmin, ApplicationRoles.RoleCustomerAdmin, ApplicationRoles.RoleTechnician, ApplicationRoles.RoleOperator, ApplicationRoles.RoleViewer, ApplicationRoles.RoleMMSBridge));
           
            options.AddPolicy(AuthorizationPolicies.RequireSysAdminOrCustAdminOrMMSBridge, policy =>
                    policy.RequireRole(ApplicationRoles.RoleSystemAdmin, ApplicationRoles.RoleCustomerAdmin, ApplicationRoles.RoleMMSBridge));


            options.AddPolicy(AuthorizationPolicies.DenyAll, policy =>
          policy.AddRequirements(new DenyAllRequirement()));


        });


        return services;
    }

    public class DenyAllRequirement : IAuthorizationRequirement
    {
    }
    public class DenyAllHandler : AuthorizationHandler<DenyAllRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, DenyAllRequirement requirement)
        {
            context.Fail();
            return Task.CompletedTask;
        }
    }
}