using Microsoft.AspNetCore.Authorization;

namespace Talapker.Infrastructure.AuthZ;

public class TenantAuthorizationRequirement : IAuthorizationRequirement
{
    public bool IsAccessibleToSystemAdmin { get; }
    public string[]? AllowedRoles { get; }

    public TenantAuthorizationRequirement(bool isAccessibleToSystemAdmin = false, string[]? allowedRoles = null)
    {
        IsAccessibleToSystemAdmin = isAccessibleToSystemAdmin;
        AllowedRoles = allowedRoles;
    }
}