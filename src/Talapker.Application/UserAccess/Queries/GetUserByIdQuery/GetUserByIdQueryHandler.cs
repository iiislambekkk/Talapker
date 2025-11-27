using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Talapker.Infrastructure.Data;
using Talapker.Infrastructure.Data.UserAccess;

namespace Talapker.Application.UserAccess.Queries.GetUserByIdQuery;

public class GetUserByIdQueryHandler()
{
    public async Task<UserDto?> Handle(GetUserByIdQuery query, UserManager<ApplicationUser> manager)
    {
        var userFromDb = await manager.FindByIdAsync(query.Id);
        if (userFromDb == null) return null;
        
        var userDto = userFromDb.ToUserDto();
        
        return userDto;
    }
}