using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Talapker.Infrastructure.Data.UserAccess;

namespace Talapker.Infrastructure.AuthZ;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class TenantAuthorizeAttribute : AuthorizeAttribute, IAuthorizationFilter
{
    public string? Roles { get; set; }
    public bool IsAccessibleToSystemAdmin { get; set; } = false;
    public string TenantIdParameter { get; set; } = "tenantId";

    public TenantAuthorizeAttribute() 
    {
        AuthenticationSchemes = "OpenIddict.Validation.AspNetCore";
    }

    public TenantAuthorizeAttribute(string roles, bool isAccessibleToSystemAdmin = false) 
        : this()
    {
        Roles = roles;
        IsAccessibleToSystemAdmin = isAccessibleToSystemAdmin;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        // Skip authorization if action allows anonymous access
        if (context.ActionDescriptor.EndpointMetadata.Any(em => em is IAllowAnonymous))
            return;

        var user = context.HttpContext.User;
        
        // Check authentication
        if (!user.Identity?.IsAuthenticated ?? false)
        {
            context.Result = new ChallengeResult();
            return;
        }
        
        

        // Check if user is System Admin and has access
        if (IsAccessibleToSystemAdmin && user.IsInRole(UserRoles.SystemAdmin))
        { 
          
            return; // System admin has access
        }

        // Get tenantId from route parameters
        if (!TryGetTenantIdFromRoute(context, out var tenantId))
        {
            context.Result = new ForbidResult();
            LogWarning(context, "Tenant ID not found in route parameters");
            return;
        }

        // Check if user has access to this tenant
        if (!HasTenantAccess(user, tenantId))
        {
            context.Result = new ForbidResult();
            LogWarning(context, $"User does not have access to tenant {tenantId}");
            return;
        }

        // Check roles if specified
        if (!string.IsNullOrEmpty(Roles) && !HasRequiredRoles(user, Roles))
        {
            context.Result = new ForbidResult();
            LogWarning(context, $"User does not have required roles: {Roles}");
            return;
        }
    }

    private bool TryGetTenantIdFromRoute(AuthorizationFilterContext context, out Guid tenantId)
    {
        tenantId = Guid.Empty;
        
        // Try to get tenantId from route values
        if (context.RouteData.Values.TryGetValue(TenantIdParameter, out var tenantIdValue))
        {
            if (tenantIdValue is Guid guidValue)
            {
                tenantId = guidValue;
                return true;
            }
            else if (Guid.TryParse(tenantIdValue?.ToString(), out var parsedTenantId))
            {
                tenantId = parsedTenantId;
                return true;
            }
        }

        return false;
    }

    private bool HasTenantAccess(ClaimsPrincipal user, Guid tenantId)
    {
        Console.WriteLine(user.FindFirstValue("tenantId"));
        
        var userTenantIdClaim = user.FindFirst("tenantId")?.Value;
        if (string.IsNullOrEmpty(userTenantIdClaim))
            return false;

        return Guid.TryParse(userTenantIdClaim, out var userTenantId) && userTenantId == tenantId;
    }

    private bool HasRequiredRoles(ClaimsPrincipal user, string roles)
    {
        var requiredRoles = roles.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(r => r.Trim())
            .ToArray();

        return requiredRoles.Any(role => user.IsInRole(role));
    }

    private void LogWarning(AuthorizationFilterContext context, string message)
    {
        var logger = context.HttpContext.RequestServices.GetService<ILogger<TenantAuthorizeAttribute>>();
        logger?.LogWarning("Authorization failed: {Message}. User: {User}, Tenant: {Tenant}",
            message, 
            context.HttpContext.User.Identity?.Name,
            context.HttpContext.User.FindFirst("tenantId")?.Value);
    }
}