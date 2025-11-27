namespace Talapker.Infrastructure.AuthZ;

public static class AuthorizationPolicies
{
    public const string SystemAdminOnly = "SystemAdminOnly";
    public const string TenantAdmins = "TenantAdmins";
    public const string AllTenantUsers = "AllTenantUsers";
}