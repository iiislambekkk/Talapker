using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Talapker.Application.Helpers;
using Talapker.Infrastructure.Data;
using Talapker.Infrastructure.Data.Institution;
using Talapker.Infrastructure.Data.UserAccess;

namespace Talapker.Application.AmbassadorFeatures.Commands;

public record AcceptAmbassadorInvitationCommand(
    Guid InvitationId,
    string SecretCode,

    string Password,
    string FirstName,
    string LastName,

    string? Tagline,
    string? Bio,
    int StudyYear,
    string? DegreeType,
    List<string>? Languages,
    List<string>? Interests
);

public class AcceptAmbassadorInvitationHandler
{
    public async Task<ApiResponse> Handle(
        AcceptAmbassadorInvitationCommand command,
        UserManager<ApplicationUser> userManager,
        TalapkerDbContext db)
    {
        var invitation = await db.Invitations
            .FirstOrDefaultAsync(i =>
                i.Id == command.InvitationId &&
                i.Role == UserRoles.UserRolesEnum.TenantAmbassador);

        if (invitation == null)
            return ApiResponse.Fail("Invitation not found.", ErrorCodes.Default);

        var (valid, error) = InvitationHelper.Validate(invitation, command.SecretCode);
        if (!valid) return ApiResponse.Fail(error!, ErrorCodes.Default);

        // 1. Resolve or create user
        var user = await userManager.FindByEmailAsync(invitation.Email);

        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = invitation.Email,
                Email = invitation.Email,
                TenantId = invitation.TenantId,
                EmailConfirmed = true,
                FirstName = command.FirstName,
                LastName = command.LastName
            };

            var createResult = await userManager.CreateAsync(user, command.Password);
            if (!createResult.Succeeded)
                return ApiResponse.Fail(
                    string.Join(", ", createResult.Errors.Select(e => e.Description)),
                    ErrorCodes.Default);

            await userManager.AddToRoleAsync(user, UserRoles.TenantAmbassador);
        }
        else
        {
            bool hasPassword = await userManager.HasPasswordAsync(user);
            if (!hasPassword)
            {
                var addPassword = await userManager.AddPasswordAsync(user, command.Password);
                if (!addPassword.Succeeded)
                    return ApiResponse.Fail(
                        string.Join(", ", addPassword.Errors.Select(e => e.Description)),
                        ErrorCodes.Default);
            }

            user.FirstName = command.FirstName;
            user.LastName = command.LastName;
            user.EmailConfirmed = true;
            await userManager.UpdateAsync(user);
        }

        // 2. Create or update ambassador profile — always keyed by UserId, never Email
        var ambassador = await db.Ambassadors
            .FirstOrDefaultAsync(a => a.UserId == user.Id);

        if (ambassador == null)
        {
            db.Ambassadors.Add(new Ambassador
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                IsActive = true,
                InstitutionId = invitation.TenantId,
                DateJoined = DateTime.UtcNow,
                LastActiveAt = DateTime.UtcNow,
                Tagline = command.Tagline,
                Bio = command.Bio,
                StudyYear = command.StudyYear,
                DegreeType = command.DegreeType,
                Languages = command.Languages ?? new List<string>(),
                Interests = command.Interests ?? new List<string>()
            });
        }
        else
        {
            ambassador.IsActive = true;
            ambassador.LastActiveAt = DateTime.UtcNow;
            ambassador.Tagline = command.Tagline;
            ambassador.Bio = command.Bio;
            ambassador.StudyYear = command.StudyYear;
            ambassador.DegreeType = command.DegreeType;
            ambassador.Languages = command.Languages ?? new List<string>();
            ambassador.Interests = command.Interests ?? new List<string>();
        }

        // 3. Finalize invitation
        invitation.Status = InvitationStatus.Accepted;
        invitation.RespondedAt = DateTime.UtcNow;
        invitation.AcceptedByUserId = Guid.Parse(user.Id);

        await db.SaveChangesAsync();
        return ApiResponse.Success();
    }
}