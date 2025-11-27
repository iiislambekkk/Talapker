using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Talapker.Application.UserAccess.Queries;
using Talapker.Infrastructure.Data;
using Talapker.Infrastructure.Data.UserAccess;

namespace Talapker.Application.InstitutionFeatures.Queries.GetPrimaryAdmin;

public class GetPrimaryAdminQueryHandler
{
    public async Task<UserDto?> Handle
    (
        GetPrimaryAdminQuery query,
        TalapkerDbContext db,
        RoleManager<ApplicationRole> roleManager
    )
    {
        var primaryAdminRole = await roleManager.FindByNameAsync(UserRoles.PrimaryTenantAdmin);
        var primaryAdminRoleId  = primaryAdminRole?.Id ?? "";
        
        var adminUser = await (
            from ur in db.UserRoles
            join u in db.Users on ur.UserId equals u.Id
            where ur.RoleId == primaryAdminRoleId && u.TenantId == query.InstitutionId
            select u
        ).FirstOrDefaultAsync();

        return adminUser?.ToUserDto();
    }
}