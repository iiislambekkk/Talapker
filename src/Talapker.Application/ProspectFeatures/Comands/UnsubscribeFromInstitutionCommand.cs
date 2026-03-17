using Microsoft.EntityFrameworkCore;
using Talapker.Infrastructure.Data;
using Talapker.Infrastructure.Exceptions;

namespace Talapker.Application.ProspectFeatures.Comands;

public record UnsubscribeFromInstitutionCommand(Guid UserId, Guid InstitutionId);

public class UnsubscribeFromInstitutionHandler
{
    public async Task Handle(
        UnsubscribeFromInstitutionCommand command,
        TalapkerDbContext db,
        CancellationToken ct = default)
    {
        var user = await db.Users
            .Include(u => u.SubscribedInstitutions)
            .FirstOrDefaultAsync(u => u.Id == command.UserId.ToString(), ct);

        if (user is null)
            throw new NotFoundException($"User {command.UserId} not found");

        var institution = user.SubscribedInstitutions
            .FirstOrDefault(i => i.Id == command.InstitutionId);

        if (institution is null)
            return;

        user.SubscribedInstitutions.Remove(institution);
        await db.SaveChangesAsync(ct);
    }
}