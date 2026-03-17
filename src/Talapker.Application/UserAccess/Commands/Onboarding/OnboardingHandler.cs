using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Talapker.Infrastructure.Data.UserAccess;

namespace Talapker.Application.UserAccess.Commands.Onboarding;

public class OnboardingHandler
{
    public async Task<ApiResponse> Handle(
        OnboardingCommand command,
        UserManager<ApplicationUser> userManager)
    {
        var user = await userManager.FindByEmailAsync(command.Email);
        if (user is null)
            return ApiResponse.Fail("User not found.", "500");

        var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(command.Code));

        var resetResult = await userManager.ResetPasswordAsync(user, decodedToken, command.NewPassword);
        if (!resetResult.Succeeded)
        {
            var errors = string.Join(", ", resetResult.Errors.Select(e => e.Description));
            return ApiResponse.Fail($"Password reset failed: {errors}", "500");
        }

        user.FirstName = command.FirstName;
        user.LastName = command.LastName;

        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
            return ApiResponse.Fail("Failed to update user profile.", "500");

        return ApiResponse.Success();
    }
}