using Talapker.Infrastructure.Data;
using Talapker.Infrastructure.Data.UserAccess;

namespace Talapker.Application.UserAccess.Queries;

public static class ApplicationUserMapper
{
    public static UserDto ToUserDto(this ApplicationUser user)
    {
        return new UserDto(user.Id, user.Email, user.FirstName, user.LastName, user.AvatarKey, user.TenantId);
    }
}