using Microsoft.EntityFrameworkCore;
using Talapker.Infrastructure.Data;
using Talapker.Infrastructure.Exceptions;

namespace Talapker.Application.ProspectFeatures.Comands;

public record SubscribeToInstitutionCommand(Guid UserId, Guid InstitutionId);

public class SubscribeToInstitutionHandler
{
    public async Task Handle(
        SubscribeToInstitutionCommand command,
        TalapkerDbContext db,
        CancellationToken ct = default)
    {
        var user = await db.Users
            .Include(u => u.SubscribedInstitutions)
            .FirstOrDefaultAsync(u => u.Id == command.UserId.ToString(), ct);

        if (user is null)
            throw new NotFoundException($"User {command.UserId} not found");

        var alreadySubscribed = user.SubscribedInstitutions
            .Any(i => i.Id == command.InstitutionId);

        if (alreadySubscribed)
            return;

        var institution = await db.Institutions
            .FindAsync([command.InstitutionId], ct);

        if (institution is null)
            throw new NotFoundException($"Institution {command.InstitutionId} not found");

        user.SubscribedInstitutions.Add(institution);
        await db.SaveChangesAsync(ct);
    }
}