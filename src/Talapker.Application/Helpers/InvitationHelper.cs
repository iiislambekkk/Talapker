using System.Text;
using Talapker.Infrastructure.Data.UserAccess;

namespace Talapker.Application.Helpers;

public static class InvitationHelper
{
    public static string ComputeHash(string input) =>
        Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(
                Encoding.UTF8.GetBytes(input)));

    public static bool VerifyCode(string rawCode, string storedHash) =>
        string.Equals(ComputeHash(rawCode), storedHash, StringComparison.OrdinalIgnoreCase);

    public static string ToRoleName(UserRoles.UserRolesEnum role) => role switch
    {
        UserRoles.UserRolesEnum.TenantAmbassador  => UserRoles.TenantAmbassador,
        UserRoles.UserRolesEnum.TenantAdmin        => UserRoles.TenantAdmin,
        UserRoles.UserRolesEnum.PrimaryTenantAdmin => UserRoles.PrimaryTenantAdmin,
        UserRoles.UserRolesEnum.SystemAdmin        => UserRoles.SystemAdmin,
        _ => throw new ArgumentOutOfRangeException(nameof(role))
    };

    public static (bool Valid, string? Error) Validate(Invitation invitation, string rawCode)
    {
        if (invitation.Status != InvitationStatus.Pending)
            return (false, $"Invitation is already {invitation.Status}.");

        if (invitation.ExpiresAt < DateTime.UtcNow)
            return (false, "Invitation has expired.");

        if (!VerifyCode(rawCode, invitation.SecretCodeHash))
            return (false, "Invalid invitation code.");

        return (true, null);
    }
}