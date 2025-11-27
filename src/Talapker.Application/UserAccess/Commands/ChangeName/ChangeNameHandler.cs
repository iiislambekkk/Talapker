using Microsoft.AspNetCore.Identity;
using Talapker.Infrastructure.Data;
using Talapker.Infrastructure.Data.UserAccess;
using Talapker.Infrastructure.Exceptions;
using Talapker.UserAccess.Application.Features.ChangeName;

namespace Talapker.Application.UserAccess.Commands.ChangeName;

public class ChangeNameHandler
{
    public async Task Handle(UserManager<ApplicationUser> userManager, ChangeNameCommand command)
    {
        var user = await userManager.FindByIdAsync(command.UserId.ToString());
        if (user is null) throw new NotFoundException($"User with Id {command.UserId} not found");

        user.FirstName = command.FirstName;
        user.LastName = command.LastName;
        
        await userManager.UpdateAsync(user);
    }
}