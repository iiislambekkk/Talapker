using Microsoft.EntityFrameworkCore;
using Talapker.Application.AmbassadorFeatures.DTOs;
using Talapker.Infrastructure.Data;

namespace Talapker.Application.AmbassadorFeatures.Queries;

public record GetAmbassadorByUserIdQuery(Guid UserId);

public class GetAmbassadorByUserIdHandler()
{
    public async Task<AmbassadorDto?> Handle(
        GetAmbassadorByUserIdQuery query,
        TalapkerDbContext db,
        CancellationToken cancellationToken)
    {
        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Id == query.UserId.ToString(), cancellationToken);

        if (user == null)
            return null;

        var ambassador = await db.Ambassadors
            .Include(a => a.EducationProgram)
            .FirstOrDefaultAsync(a => a.Email == user.Email, cancellationToken);
        
        return ambassador?.ToDto();
    }
}