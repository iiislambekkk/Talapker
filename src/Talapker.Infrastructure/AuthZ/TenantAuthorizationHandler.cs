using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Talapker.Infrastructure.Data.UserAccess;

namespace Talapker.Infrastructure.AuthZ;

public class TenantAuthorizationHandler : AuthorizationHandler<TenantAuthorizationRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        TenantAuthorizationRequirement requirement)
    {
        if (requirement.IsAccessibleToSystemAdmin && context.User.IsInRole(UserRoles.SystemAdmin))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        if (!TryGetTenantIdFromRoute(context, out var tenantId))
        {
            return Task.CompletedTask;
        }

        if (!HasTenantAccess(context.User, tenantId))
        {
            return Task.CompletedTask;
        }

        if (requirement.AllowedRoles != null && requirement.AllowedRoles.Length > 0)
        {
            if (!HasRequiredRoles(context.User, requirement.AllowedRoles))
            {
                return Task.CompletedTask;
            }
        }

        context.Succeed(requirement);
        return Task.CompletedTask;
    }

    private static bool TryGetTenantIdFromRoute(AuthorizationHandlerContext context, out Guid tenantId)
    {
        tenantId = Guid.Empty;
        
        if (context.Resource is HttpContext httpContext)
        {
            var routeData = httpContext.GetRouteData();
            if (routeData.Values.TryGetValue("tenantId", out var tenantIdValue) &&
                Guid.TryParse(tenantIdValue?.ToString(), out var parsedTenantId))
            {
                tenantId = parsedTenantId;
                return true;
            }
        }
        
        return false;
    }

    private static bool HasTenantAccess(ClaimsPrincipal user, Guid tenantId)
    {
        var userTenantIdClaim = user.FindFirst("tenantId")?.Value;
        return Guid.TryParse(userTenantIdClaim, out var userTenantId) && userTenantId == tenantId;
    }

    private static bool HasRequiredRoles(ClaimsPrincipal user, string[] requiredRoles)
    {
        return requiredRoles.Any(role => user.IsInRole(role));
    }
}