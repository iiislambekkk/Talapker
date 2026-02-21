using Microsoft.EntityFrameworkCore;
using Talapker.Application.FacultyFeatures.DTOs;
using Talapker.Infrastructure.Data;

namespace Talapker.Application.FacultyFeatures.Queries;

public record GetAllFacultiesWithProgramsQuery(Guid InstitutionId);

public class GetFacultyWithProgramsQueryHandler
{
    public async Task<ApiResponse<List<FacultyWithProgramsDto>>> Handle(
        GetAllFacultiesWithProgramsQuery query,
        TalapkerDbContext db,
        CancellationToken cancellationToken)
    {
        var faculties = await db.Faculties
            .Include(f => f.EducationPrograms)
            .Where(f => f.InstitutionId == query.InstitutionId)
            .ToListAsync(cancellationToken);

        var result = faculties.Select(f => new FacultyWithProgramsDto
        {
            Id = f.Id,
            Name = f.Name,
            EducationPrograms = f.EducationPrograms
                .Select(p => new EducationProgramSimpleDto
                {
                    Id = p.Id,
                    Code = p.Code,
                    Name = p.Name
                })
                .ToList()
        }).ToList();

        return ApiResponse<List<FacultyWithProgramsDto>>.Success(result);
    }
}