using Microsoft.EntityFrameworkCore;
using Talapker.Application.FacultyFeatures.DTOs;
using Talapker.Application.FacultyFeatures.DTOs.Mappers;
using Talapker.Infrastructure;
using Talapker.Infrastructure.Data;
using Talapker.Infrastructure.Data.Institution;

namespace Talapker.Application.FacultyFeatures.Queries.GetEducationProgramsQuery;

public class GetEducationProgramsHandler
{
    public async Task<List<EducationProgramDto>> Handle(
        GetEducationProgramsQuery query,
        TalapkerDbContext db,
        CancellationToken cancellationToken)
    {
        var q = db.EducationPrograms
            .AsNoTracking()
            .Include(p => p.Faculty)
            .Include(p => p.EducationGroup)
            .Where(p => p.Faculty!.InstitutionId == query.InstitutionId);

        if (query.FacultyId.HasValue)
            q = q.Where(p => p.FacultyId == query.FacultyId);

        return await q
            .Select(p => p.ToDto())
            .ToListAsync(cancellationToken);
    }
}