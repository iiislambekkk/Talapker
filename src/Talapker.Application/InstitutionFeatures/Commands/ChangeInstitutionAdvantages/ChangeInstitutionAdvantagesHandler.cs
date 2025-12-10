using Microsoft.EntityFrameworkCore;
using Talapker.Infrastructure.Data;

namespace Talapker.Application.InstitutionFeatures.Commands.ChangeInstitutionAdvantages;

public class ChangeInstitutionAdvantagesHandler
{
    public async Task<ApiResponse> Handle(ChangeInstitutionAdvantagesCommand command, TalapkerDbContext db, CancellationToken cancellationToken)
    {
        var institution = await db.Institutions.FirstOrDefaultAsync(a => a.Id == command.Id, cancellationToken);

        if (institution == null)
        {
            return ApiResponse.Fail("Institution not found", "InstitutionNotFound", 404);
        }

        institution.Advantages = command.Advantages;
        await db.SaveChangesAsync(cancellationToken);
        
        return ApiResponse.Success();
    }
}