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
        var ambassador = await db.Ambassadors
            .Include(a => a.EducationProgram)
            .Include(a => a.User)
            .Include(a => a.Institution)
            .FirstOrDefaultAsync(a => a.UserId == query.UserId.ToString(), cancellationToken);
        
        return ambassador?.ToDto();
    }
}