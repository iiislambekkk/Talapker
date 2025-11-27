using System.Net;
using System.Text;
using System.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Talapker.Infrastructure.Data;
using Talapker.Infrastructure.Data.UserAccess;

namespace Talapker.Application.InstitutionFeatures.Commands.AssignPrimaryTenantAdmin;

public class AssignPrimaryTenantAdminHandler
{
    public async Task<ApiResponse> Handle
    (
        AssignPrimaryTenantAdminCommand command,
        UserManager<ApplicationUser> userManager,
        IEmailSender emailSender,
        IConfiguration configuration,
        RoleManager<ApplicationRole> roleManager,
        TalapkerDbContext db
    )
    {
        var primaryAdminRole = await roleManager.FindByNameAsync(UserRoles.PrimaryTenantAdmin);
        var primaryAdminRoleId  = primaryAdminRole?.Id ?? "";
        
        bool alreadyExists = await (
            from ur in db.UserRoles
            join u in db.Users on ur.UserId equals u.Id
            where ur.RoleId == primaryAdminRoleId && u.TenantId == command.InstitutionId
            select ur
        ).AnyAsync();

        if (alreadyExists)
            return ApiResponse.Fail("Tenant already has a primary admin.", ErrorCodes.TenantAlreadyHasPrimaryAdmin);
        
        var existingUser = await userManager.Users.FirstOrDefaultAsync(u => u.Email == command.Email);
        
        if (existingUser != null)
        {
            if (existingUser.TenantId is not null)
            {
                if (existingUser.TenantId != command.InstitutionId)
                {
                    return ApiResponse.Fail("User already exist and involved in another tenant.", ErrorCodes.UserAlreadyInvolvedToAnotherTenant);
                }

                await userManager.AddToRoleAsync(existingUser, UserRoles.PrimaryTenantAdmin);
                
                return ApiResponse.Success();
            }
        }
        
        var newUser = new ApplicationUser
        {
            UserName = command.Email,
            Email = command.Email, 
            TenantId = command.InstitutionId,
            EmailConfirmed = true,
            FirstName = command.Email
        };
        
        var createResult = await userManager.CreateAsync(newUser);
        if (!createResult.Succeeded)
            return ApiResponse.Fail("User already exist and involved in another tenant.", ErrorCodes.UserAlreadyInvolvedToAnotherTenant);
        
        await userManager.AddToRoleAsync(newUser, UserRoles.PrimaryTenantAdmin);
        
        var resetToken = await userManager.GeneratePasswordResetTokenAsync(newUser);

        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(resetToken));
        var encodedEmail = WebUtility.UrlEncode(command.Email);
        var resetUrl = $"{configuration["ServiceSettings:AppUrl"]}/Identity/Account/ResetPassword?code={encodedToken}&email={encodedEmail}";

        await emailSender.SendEmailAsync(
            command.Email,
            "Welcome to the platform",
            $"Please set your password by <a href='{resetUrl}'>clicking here</a>.");
        
        return  ApiResponse.Success();
    }
}