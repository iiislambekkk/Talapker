using Microsoft.EntityFrameworkCore;
using Talapker.Infrastructure.Data;
using Talapker.Infrastructure.Data.Institution;
using Talapker.Infrastructure.Data.Institution.InstitutionEntity;

namespace Talapker.Application.FacultyFeatures.Queries.GetAllEducationGroups;

public class GetAllEducationGroupsQueryHandler()
{
    public async Task<List<EducationGroup>> Handle(GetAllEducationGroupsQuery query, TalapkerDbContext db, CancellationToken cancellationToken)
    {
        return await db.EducationGroups
            .ToListAsync(cancellationToken);
    }
}