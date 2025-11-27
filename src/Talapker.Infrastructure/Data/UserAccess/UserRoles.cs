namespace Talapker.Infrastructure.Data.UserAccess;

public static class UserRoles
{
    public const string SystemAdmin = "SystemAdmin";
    public const string TenantAmbassador = "TenantAmbassador";
    public const string TenantAdmin = "TenantAdmin";
    public const string PrimaryTenantAdmin = "PrimaryTenantAdmin";

    public static readonly string[] AllRoles = 
    {
        SystemAdmin, 
        TenantAmbassador,
        TenantAdmin, 
        PrimaryTenantAdmin
    };

    public static readonly string[] TenantAdminRoles =
    {
        TenantAdmin,
        PrimaryTenantAdmin
    };

    public static readonly string[] AllTenantRoles =
    {
        TenantAmbassador,
        TenantAdmin,
        PrimaryTenantAdmin
    };

    public static List<string> AsList() => AllRoles.ToList();

    // Helper methods for role combinations
    public static string TenantAdmins => string.Join(",", TenantAdminRoles);
    public static string AllTenant => string.Join(",", AllTenantRoles);
}