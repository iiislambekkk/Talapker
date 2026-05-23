using Microsoft.EntityFrameworkCore;
using Talapker.Application.FacultyFeatures.DTOs.Mappers;
using Talapker.Infrastructure.Data;

namespace Talapker.Application.FacultyFeatures.Queries;

public record GetEducationProgramByCodeQuery(
    string? EducationProgramCode
);

public class GetEducationProgramByCodeQueryHandler
{
    public async Task<EducationProgramDto?> Handle(
        GetEducationProgramByCodeQuery query,
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
            .FirstOrDefaultAsync(p => p.Code == query.EducationProgramCode, cancellationToken);

        return program?.ToDto();
    }
}