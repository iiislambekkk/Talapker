using System.Net;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Talapker.Infrastructure.Data;
using Talapker.Infrastructure.Data.UserAccess;
using Talapker.Notifications.Contracts;
using Wolverine;

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
        IMessageBus bus,
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
                await userManager.UpdateSecurityStampAsync(existingUser);
                
                
                await bus.PublishAsync(new SendPushCommand(
                  UserId: Guid.Parse(existingUser.Id),
                  Title: "You've been appointed as Institution Admin",
                  Body: "You now have administrative access to your institution on Talapker.",
                  Type: "RoleAssigned"
                ));
                
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
        
        var createResult = await userManager.CreateAsync(newUser, command.Email);
        if (!createResult.Succeeded)
            return ApiResponse.Fail("User already exist and involved in another tenant.", ErrorCodes.UserAlreadyInvolvedToAnotherTenant);
        
        await userManager.AddToRoleAsync(newUser, UserRoles.PrimaryTenantAdmin);
        
        var resetToken = await userManager.GeneratePasswordResetTokenAsync(newUser);

        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(resetToken));
        var encodedEmail = WebUtility.UrlEncode(command.Email);
        var resetUrl = $"{configuration["IdentitySettings:ClientAppUrl"]}/onboarding?code={encodedToken}&email={encodedEmail}&type=institutionAdmin&institutionId={command.InstitutionId}";

        var emailBody = $@"
<table width='100%' cellpadding='0' cellspacing='0' style='padding:40px 0;background:#f4f6f9;font-family:Segoe UI,Arial,sans-serif;'>
  <tr><td align='center'>

    <!-- Header -->
    <table width='560' cellpadding='0' cellspacing='0' style='background:linear-gradient(135deg,#1a56db,#0e3fa8);border-radius:16px 16px 0 0;overflow:hidden;'>
      <tr>
        <td style='padding:40px;text-align:center;'>
          <h1 style='margin:0;color:#fff;font-size:28px;font-weight:700;'>Talapker</h1>
          <p style='margin:6px 0 0;color:rgba(255,255,255,0.7);font-size:12px;text-transform:uppercase;letter-spacing:1px;'>Education Management Platform</p>
        </td>
      </tr>
    </table>

    <!-- EN Card -->
    <table width='560' cellpadding='0' cellspacing='0' style='background:#fff;margin-top:2px;'>
      <tr>
        <td style='padding:32px 40px;border-left:4px solid #1a56db;'>
          <p style='margin:0 0 4px;font-size:11px;font-weight:700;color:#1a56db;text-transform:uppercase;letter-spacing:1px;'>English</p>
          <h2 style='margin:0 0 12px;font-size:20px;color:#111827;font-weight:700;'>Welcome to Talapker! 👋</h2>
          <p style='margin:0;font-size:14px;color:#4b5563;line-height:1.7;'>
            You have been appointed as the <strong>Primary Administrator</strong> of your institution on the Talapker platform. 
            Please complete your account setup by setting your password and filling in your profile details.
          </p>
        </td>
      </tr>
    </table>

    <!-- RU Card -->
    <table width='560' cellpadding='0' cellspacing='0' style='background:#fff;margin-top:2px;'>
      <tr>
        <td style='padding:32px 40px;border-left:4px solid #0e3fa8;'>
          <p style='margin:0 0 4px;font-size:11px;font-weight:700;color:#0e3fa8;text-transform:uppercase;letter-spacing:1px;'>Русский</p>
          <h2 style='margin:0 0 12px;font-size:20px;color:#111827;font-weight:700;'>Добро пожаловать в Talapker! 👋</h2>
          <p style='margin:0;font-size:14px;color:#4b5563;line-height:1.7;'>
            Вы назначены <strong>главным администратором</strong> вашего учебного заведения на платформе Talapker.
            Пожалуйста, завершите настройку аккаунта — установите пароль и заполните профиль.
          </p>
        </td>
      </tr>
    </table>

    <!-- KK Card -->
    <table width='560' cellpadding='0' cellspacing='0' style='background:#fff;margin-top:2px;'>
      <tr>
        <td style='padding:32px 40px;border-left:4px solid #1e429f;'>
          <p style='margin:0 0 4px;font-size:11px;font-weight:700;color:#1e429f;text-transform:uppercase;letter-spacing:1px;'>Қазақша</p>
          <h2 style='margin:0 0 12px;font-size:20px;color:#111827;font-weight:700;'>Talapker-ге қош келдіңіз! 👋</h2>
          <p style='margin:0;font-size:14px;color:#4b5563;line-height:1.7;'>
            Сіз Talapker платформасында мекеменің <strong>бас әкімшісі</strong> етіп тағайындалдыңыз.
            Құпия сөзді орнату және профильді толтыру арқылы аккаунтыңызды баптауды аяқтаңыз.
          </p>
        </td>
      </tr>
    </table>

    <!-- CTA Card -->
    <table width='560' cellpadding='0' cellspacing='0' style='background:#fff;margin-top:2px;border-radius:0 0 16px 16px;overflow:hidden;'>
      <tr>
        <td style='padding:32px 40px;text-align:center;'>
          <a href='{resetUrl}' style='display:inline-block;background:linear-gradient(135deg,#1a56db,#0e3fa8);color:#fff;text-decoration:none;font-size:15px;font-weight:600;padding:14px 40px;border-radius:10px;'>
            Complete Setup &nbsp;/&nbsp; Настроить аккаунт &nbsp;/&nbsp; Аккаунтты баптау →
          </a>
          <p style='margin:20px 0 0;font-size:12px;color:#9ca3af;line-height:1.8;'>
            Link expires in 24 hours &nbsp;·&nbsp; Ссылка действительна 24 часа &nbsp;·&nbsp; Сілтеме 24 сағат жарамды.
          </p>
        </td>
      </tr>
      <tr>
        <td style='background:#f9fafb;padding:16px 40px;text-align:center;border-top:1px solid #f3f4f6;'>
          <p style='margin:0;font-size:12px;color:#9ca3af;'>© 2025 Talapker</p>
        </td>
      </tr>
    </table>

  </td></tr>
</table>";

await emailSender.SendEmailAsync(
    command.Email,
    "Welcome to Talapker / Добро пожаловать / Қош келдіңіз",
    emailBody);
        
        return  ApiResponse.Success();
    }
}