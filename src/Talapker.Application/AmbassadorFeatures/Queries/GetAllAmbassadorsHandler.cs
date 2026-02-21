using Microsoft.EntityFrameworkCore;
using Talapker.Application.AmbassadorFeatures.Commands.Queries;
using Talapker.Application.AmbassadorFeatures.DTOs;
using Talapker.Infrastructure.Data;
using Talapker.Infrastructure.Data.Institution;

namespace Talapker.Application.AmbassadorFeatures.Queries;

public class GetAllAmbassadorsHandler
{
    public async Task<List<AmbassadorDto>> Handle
    (
        GetAllAmbassadorsQuery query,
        TalapkerDbContext db,
        CancellationToken cancellationToken
    )
    {
        var ambassadorsQuery = db.Ambassadors
            .Include(a => a.EducationProgram)
            .AsQueryable();
        
        if (query.TenantId.HasValue)
        {
            ambassadorsQuery = ambassadorsQuery
                .Where(a => db.Users.Any(u => u.Email == a.Email && u.TenantId == query.TenantId));
        }
        
        return await ambassadorsQuery
            .Select(a => a.ToDto())
            .ToListAsync(cancellationToken);
    }
}