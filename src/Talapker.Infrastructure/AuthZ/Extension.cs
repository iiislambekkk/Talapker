using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using Talapker.Infrastructure.Data.UserAccess;

namespace Talapker.Infrastructure.AuthZ;

public static class Extension
{
    public static IServiceCollection AddAuthZ(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("SystemAdminOnly", policy =>
                {
                    policy.RequireRole(UserRoles.SystemAdmin);
                    policy.AuthenticationSchemes = ["OpenIddict.Validation.AspNetCore"];
                }
            );
        
            options.AddPolicy("TenantAdmins", policy =>
                {
                    policy.RequireRole(UserRoles.TenantAdmin, UserRoles.PrimaryTenantAdmin);
                    policy.AuthenticationSchemes = ["OpenIddict.Validation.AspNetCore"];
                }
            );
        
            options.AddPolicy("AllTenantUsers", policy =>
                {
                    policy.RequireRole(UserRoles.TenantAmbassador, UserRoles.TenantAdmin, UserRoles.PrimaryTenantAdmin);
                    policy.AuthenticationSchemes = ["OpenIddict.Validation.AspNetCore"];
                }
            );
        });
        
        services.AddSingleton<IAuthorizationHandler, TenantAuthorizationHandler>();
        
        return services;
    }
}