using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Talapker.Application;
using Talapker.Application.Helpers;
using Talapker.Infrastructure.Data;
using Talapker.Infrastructure.Data.Institution;
using Talapker.Infrastructure.Data.UserAccess;

public record SendInvitationCommand(
    string Email,
    Guid TenantId,
    Guid InvitedByUserId,
    UserRoles.UserRolesEnum Role
);

public class SendInvitationHandler
{
    public async Task<ApiResponse> Handle(
        SendInvitationCommand command,
        UserManager<ApplicationUser> userManager,
        IEmailSender emailSender,
        IConfiguration configuration,
        TalapkerDbContext db)
    {
        var roleName = InvitationHelper.ToRoleName(command.Role);

        // 1. Block duplicate active invitation
        bool duplicateExists = await db.Invitations.AnyAsync(i =>
            i.Email == command.Email &&
            i.TenantId == command.TenantId &&
            i.Role == command.Role &&
            i.Status == InvitationStatus.Pending &&
            i.ExpiresAt > DateTime.UtcNow);

        if (duplicateExists)
            return ApiResponse.Fail(
                $"An active {command.Role} invitation already exists for this email.",
                ErrorCodes.Default);

        // 2. Check existing user
        var existingUser = await userManager.Users
            .FirstOrDefaultAsync(u => u.Email == command.Email);

        if (existingUser != null)
        {
            bool hasRole    = await userManager.IsInRoleAsync(existingUser, roleName);
            bool sameTenant = existingUser.TenantId == command.TenantId;

            if (hasRole && sameTenant)
                return ApiResponse.Fail(
                    $"User is already a {command.Role} in this tenant.",
                    ErrorCodes.Default);

            if (existingUser.TenantId != null && !sameTenant)
                return ApiResponse.Fail(
                    "User belongs to a different tenant.",
                    ErrorCodes.Default);

            // User exists in same tenant — assign role and create profile directly
            if (!hasRole)
                await userManager.AddToRoleAsync(existingUser, roleName);

            if (command.Role == UserRoles.UserRolesEnum.TenantAmbassador)
            {
                bool profileExists = await db.Ambassadors
                    .AnyAsync(a => a.UserId == existingUser.Id);

                if (!profileExists)
                {
                    db.Ambassadors.Add(new Ambassador
                    {
                        Id = Guid.NewGuid(),
                        UserId = existingUser.Id,
                        DateJoined = DateTime.UtcNow,
                        IsActive = true,
                        InstitutionId = command.TenantId,
                        StudyYear = 1,
                        Languages = new List<string>(),
                        Interests = new List<string>()
                    });
                }
                else
                {
                    var ambassador = await db.Ambassadors
                        .FirstAsync(a => a.UserId == existingUser.Id);
                    ambassador.IsActive = true;
                }
            }

            db.Invitations.Add(new Invitation
            {
                Id = Guid.NewGuid(),
                Email = command.Email,
                TenantId = command.TenantId,
                Role = command.Role,
                Status = InvitationStatus.Accepted,
                SecretCodeHash = InvitationHelper.ComputeHash(Guid.NewGuid().ToString("N")),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow,
                RespondedAt = DateTime.UtcNow,
                InvitedByUserId = command.InvitedByUserId,
                AcceptedByUserId = Guid.Parse(existingUser.Id)
            });

            await db.SaveChangesAsync();
            return ApiResponse.Success();
        }

        // 3. New user — just create the invitation, profile created on acceptance
        var rawCode    = Guid.NewGuid().ToString("N");
        var invitation = new Invitation
        {
            Id = Guid.NewGuid(),
            Email = command.Email,
            TenantId = command.TenantId,
            Role = command.Role,
            Status = InvitationStatus.Pending,
            SecretCodeHash = InvitationHelper.ComputeHash(rawCode),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            InvitedByUserId = command.InvitedByUserId
        };

        db.Invitations.Add(invitation);
        await db.SaveChangesAsync();

        var appUrl = configuration["ServiceSettings:ClientAppUrl"];
        await SendEmail(command.Email, command.Role, appUrl!, invitation.Id, rawCode, emailSender);

        return ApiResponse.Success();
    }

    private static async Task SendEmail(
        string email,
        UserRoles.UserRolesEnum role,
        string appUrl,
        Guid invitationId,
        string rawCode,
        IEmailSender emailSender)
    {
        var link = $"{appUrl}/invitation?code={rawCode}&invitationId={invitationId}&role={role}";

        var (subject, body) = role switch
        {
            UserRoles.UserRolesEnum.TenantAmbassador => (
                "You're invited to become an Ambassador!",
                """
                <h2>You've been invited to the Ambassador Program!</h2>
                <p>You've been selected to represent your institution as a student ambassador.</p>
                """
            ),
            UserRoles.UserRolesEnum.TenantAdmin or
            UserRoles.UserRolesEnum.PrimaryTenantAdmin => (
                "You've been invited as an Admin",
                """
                <h2>You've been granted admin access!</h2>
                <p>You have been invited to manage your institution's portal.</p>
                """
            ),
            _ => (
                "You've been invited",
                "<h2>You've been invited to join the platform.</h2>"
            )
        };

        await emailSender.SendEmailAsync(email, subject, $"""
            {body}
            <p><a href='{link}'>Accept Invitation</a></p>
            <p>This link expires in 7 days. If you didn't expect this, ignore this email.</p>
            """);
    }
}