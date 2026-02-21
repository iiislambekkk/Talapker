using Microsoft.EntityFrameworkCore;
using Talapker.Application.FacultyFeatures.DTOs.Mappers;
using Talapker.Infrastructure.Data;

namespace Talapker.Application.FacultyFeatures.Queries;

public record GetEducationProgramByIdQuery(
    Guid InstitutionId,
    Guid? EducationProgramId
);

public class GetEducationProgramByIdHandler
{
    public async Task<EducationProgramDto?> Handle(
        GetEducationProgramByIdQuery query,
        TalapkerDbContext db,
        CancellationToken cancellationToken)
    {
        var program = await db.EducationPrograms
            .AsNoTracking()
            .AsSplitQuery()
            .Include(p => p.Faculty)
            .Include(p => p.EducationGroup)
            .ThenInclude(g => g.GrantCompetitionStatistics)
            .Include(p => p.EducationGroup)
            .ThenInclude(g => g.UntSubjectsPairs)
            .ThenInclude(p => p.FirstSubject)
            .Include(p => p.EducationGroup)
            .ThenInclude(g => g.UntSubjectsPairs)
            .ThenInclude(p => p.SecondSubject)
            .FirstOrDefaultAsync(p => p.Id == query.EducationProgramId, cancellationToken);

        return program?.ToDto();
    }
}