using System.Net;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Talapker.Application.AmbassadorFeatures.InviteAmbassador;
using Talapker.Infrastructure.Data;
using Talapker.Infrastructure.Data.Institution;
using Talapker.Infrastructure.Data.UserAccess;

namespace Talapker.Application.AmbassadorFeatures.Commands.InviteAmbassador;

public class InviteAmbassadorHandler
{
    public async Task<ApiResponse> Handle
    (
        InviteAmbassadorCommand command,
        UserManager<ApplicationUser> userManager,
        IEmailSender emailSender,
        IConfiguration configuration,
        RoleManager<ApplicationRole> roleManager,
        TalapkerDbContext db
    )
    {
        var ambassadorRole = await roleManager.FindByNameAsync(UserRoles.TenantAmbassador);
        var ambassadorRoleId = ambassadorRole?.Id ?? "";
        
        // 1. ВСЕГДА создаем или получаем профиль амбассадора
        var ambassador = await db.Ambassadors
            .FirstOrDefaultAsync(a => a.Email == command.Email);
            
        if (ambassador == null)
        {
            ambassador = new Ambassador
            {
                Id = Guid.NewGuid(),
                Email = command.Email,
                FullName = "",
                HasCompletedOnboarding = false,
                DateJoined = DateTime.UtcNow,
                IsActive = false,
                InstitutionId = command.TenantId,
                StudyYear = 1,
                Languages = new List<string>(),
                Interests = new List<string>()
            };
            
            db.Ambassadors.Add(ambassador);
            await db.SaveChangesAsync();
        }
        
        // 2. Проверяем наличие пользователя и роли
        var existingUser = await userManager.Users.FirstOrDefaultAsync(u => u.Email == command.Email);
        
        // Переменная для хранения токена и URL
        string? setupUrl = null;
        ApplicationUser targetUser;
        
        // Если пользователь существует и имеет роль амбассадора в этом тенанте
        if (existingUser != null)
        {
            bool hasAmbassadorRole = await userManager.IsInRoleAsync(existingUser, UserRoles.TenantAmbassador);
            bool isInCorrectTenant = existingUser.TenantId == command.TenantId;
            
            if (hasAmbassadorRole && isInCorrectTenant)
            {
                return ApiResponse.Fail("User is already an ambassador in this tenant.", ErrorCodes.Default);
            }
            
            // Если пользователь в другом тенанте
            if (existingUser.TenantId is not null && existingUser.TenantId != command.TenantId)
            {
                return ApiResponse.Fail("User already exists and is involved in another tenant.", ErrorCodes.Default);
            }
            
            // Обновляем TenantId если нужно
            if (existingUser.TenantId != command.TenantId)
            {
                existingUser.TenantId = command.TenantId;
                await userManager.UpdateAsync(existingUser);
            }
            
            // Добавляем роль амбассадора если её нет
            if (!hasAmbassadorRole)
            {
                await userManager.AddToRoleAsync(existingUser, UserRoles.TenantAmbassador);
            }
            
            // Обновляем профиль амбассадора
            ambassador.FullName = $"{existingUser.FirstName} {existingUser.LastName}".Trim();
            ambassador.IsActive = true;
            await db.SaveChangesAsync();
            
            targetUser = existingUser;
        }
        else
        {
            // Создаем нового пользователя
            var newUser = new ApplicationUser
            {
                UserName = command.Email,
                Email = command.Email,
                TenantId = command.TenantId,
                EmailConfirmed = false,
                FirstName = "",
                LastName = ""
            };
            
            var createResult = await userManager.CreateAsync(newUser);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                return ApiResponse.Fail($"Failed to create user: {errors}", ErrorCodes.Default);
            }
            
            await userManager.AddToRoleAsync(newUser, UserRoles.TenantAmbassador);
            
            // Обновляем профиль амбассадора
            ambassador.FullName = command.Email;
            ambassador.IsActive = false;
            await db.SaveChangesAsync();
            
            targetUser = newUser;
        }
        
        // 4. ВСЕГДА отправляем email (для всех случаев)
        var resetToken = await userManager.GeneratePasswordResetTokenAsync(targetUser);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(resetToken));
        var encodedEmail = WebUtility.UrlEncode(command.Email);
        
        setupUrl = $"{configuration["ServiceSettings:AppUrl"]}/Identity/Account/ResetPassword?code={encodedToken}&email={encodedEmail}&tenantId={command.TenantId}";
        
        await emailSender.SendEmailAsync(
            command.Email,
            "You're invited to become an Ambassador!",
            $@"
            <h2>Welcome to the Ambassador Program!</h2>
            <p>You've been invited to become an ambassador for your institution.</p>
            <p>Please complete your registration by <a href='{setupUrl}'>clicking here</a>.</p>
            <p>This link will expire in 7 days.</p>
            ");
        
        return ApiResponse.Success();
    }
}