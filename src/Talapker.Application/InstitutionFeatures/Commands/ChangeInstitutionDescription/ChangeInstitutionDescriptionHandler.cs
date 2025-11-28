using Microsoft.EntityFrameworkCore;
using Talapker.Infrastructure.Data;

namespace Talapker.Application.InstitutionFeatures.Commands.ChangeInstitutionDescription;

public class ChangeInstitutionDescriptionHandler
{
    public async Task Handle(ChangeInstitutionDescriptionCommand command, TalapkerDbContext db, CancellationToken ct)
    {
        var institution = await db.Institutions
            .Include(a => a.Description)
            .FirstOrDefaultAsync(i => i.Id == command.Id, ct);

        if (institution != null)
        {
            institution.Description = command.Description;
            await db.SaveChangesAsync(ct);
        }
    }
}