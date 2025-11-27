using Microsoft.AspNetCore.Identity;
using Talapker.Infrastructure.Data;
using Talapker.Infrastructure.Data.UserAccess;
using Talapker.Infrastructure.Exceptions;

namespace Talapker.Application.UserAccess.Queries.GetUserInfo;

public class GetUserInfoHandler
{
    public async Task<UserInfoDto> Handle(GetUserInfoRequest request, UserManager<ApplicationUser> userManager)
    {
        var user = await userManager.FindByIdAsync(request.UserId);
        if (user is null) throw new NotFoundException($"User with Id {request.UserId} not found");
        
        var roles = (await userManager.GetRolesAsync(user).ConfigureAwait(false)).ToList();
        
        return new UserInfoDto(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email ?? "",
            user.AvatarKey ?? "",
            roles,
            user.TenantId
        );
    }
}