using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Talapker.Infrastructure.Data;
using Talapker.Infrastructure.Data.UserAccess;

namespace Talapker.Application.UserAccess.Queries.GetAllUsers;

public class GetAllUsersQueryHandler()
{
    public async Task<List<UserDto>> Handle(GetAllUsersQuery query, UserManager<ApplicationUser> manager)
    {
        var usersFromDb = await manager.Users.Select(u => u.ToUserDto()).ToListAsync();
        
        return usersFromDb;
    }
}