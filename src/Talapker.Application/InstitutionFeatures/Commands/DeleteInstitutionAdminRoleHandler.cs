using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Talapker.Infrastructure.Data.UserAccess;

namespace Talapker.Application.InstitutionFeatures.Commands;

public record DeleteInstitutionAdminRoleCommand(Guid UserId, Guid InstitutionId);

public class DeleteInstitutionAdminRoleHandler
{
    public async Task<ApiResponse> Handle(
        DeleteInstitutionAdminRoleCommand command,
        UserManager<ApplicationUser> userManager)
    {
        var user = await userManager.Users
            .FirstOrDefaultAsync(u => u.Id == command.UserId.ToString() && u.TenantId == command.InstitutionId);

        if (user is null)
            return ApiResponse.Fail("User not found in this institution.", ErrorCodes.Default);

        var isAdmin = await userManager.IsInRoleAsync(user, UserRoles.PrimaryTenantAdmin);
        if (!isAdmin)
            return ApiResponse.Fail("User does not have the admin role.", ErrorCodes.Default);

        var result = await userManager.RemoveFromRoleAsync(user, UserRoles.PrimaryTenantAdmin);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return ApiResponse.Fail($"Failed to remove role: {errors}", ErrorCodes.Default);
        }
        
        await userManager.UpdateSecurityStampAsync(user);
        

        return ApiResponse.Success();
    }
}