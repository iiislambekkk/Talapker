using Microsoft.EntityFrameworkCore;
using Talapker.Application.AmbassadorFeatures.DTOs;
using Talapker.Infrastructure.Data;
using Talapker.Infrastructure.Data.Institution;

namespace Talapker.Application.AmbassadorFeatures.Queries;

public record GetAllAmbassadorsQuery(Guid? TenantId = null);


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
            .Include(a => a.User)
            .Include(a => a.Institution)
            .AsQueryable();
        
        if (query.TenantId.HasValue)
        {
            ambassadorsQuery = ambassadorsQuery
                .Where(a => a.InstitutionId == query.TenantId);
        }
        
        return await ambassadorsQuery
            .Select(a => a.ToDto())
            .ToListAsync(cancellationToken);
    }
}