using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Talapker.Application.Helpers;
using Talapker.Infrastructure.Data;
using Talapker.Infrastructure.Data.UserAccess;

namespace Talapker.Application.UserAccess.Commands;

public record DeclineInvitationCommand(
    Guid InvitationId,
    string SecretCode
);

public class DeclineInvitationHandler
{
    public async Task<ApiResponse> Handle(
        DeclineInvitationCommand command,
        UserManager<ApplicationUser> userManager,
        TalapkerDbContext db)
    {
        var invitation = await db.Invitations
            .FirstOrDefaultAsync(i => i.Id == command.InvitationId);

        if (invitation == null)
            return ApiResponse.Fail("Invitation not found.", ErrorCodes.Default);

        var (valid, error) = InvitationHelper.Validate(invitation, command.SecretCode);
        if (!valid) return ApiResponse.Fail(error!, ErrorCodes.Default);

        invitation.Status = InvitationStatus.Declined;
        invitation.RespondedAt = DateTime.UtcNow;

        // If user already existed and was pre-assigned the role, revoke it
        if (invitation.Role == UserRoles.UserRolesEnum.TenantAmbassador)
        {
            var user = await userManager.FindByEmailAsync(invitation.Email);
            if (user != null)
            {
                bool hasRole = await userManager.IsInRoleAsync(user, UserRoles.TenantAmbassador);
                if (hasRole)
                    await userManager.RemoveFromRoleAsync(user, UserRoles.TenantAmbassador);

                // Deactivate profile if it exists
                var ambassador = await db.Ambassadors
                    .FirstOrDefaultAsync(a => a.UserId == user.Id);
                if (ambassador != null)
                    ambassador.IsActive = false;
            }
        }

        await db.SaveChangesAsync();
        return ApiResponse.Success();
    }
}