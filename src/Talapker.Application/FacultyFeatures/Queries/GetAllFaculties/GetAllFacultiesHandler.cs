using Microsoft.EntityFrameworkCore;
using Talapker.Application.FacultyFeatures.DTOs;
using Talapker.Application.FacultyFeatures.DTOs.Mappers;
using Talapker.Infrastructure.Data;
using Talapker.Infrastructure.Data.Institution;

namespace Talapker.Application.FacultyFeatures.Queries.GetAllFaculties;

public class GetAllFacultiesHandler()
{
    public async Task<List<FacultyDto>> Handle(
        GetAllFacultiesQuery query, 
        TalapkerDbContext db, 
        CancellationToken cancellationToken)
    {
        var faculties = await db.Faculties
            .Where(f => f.InstitutionId == query.InstitutionId)
            .Include(f => f.EducationPrograms)
            .ToListAsync(cancellationToken);

        return faculties.ToDtoList();
    }
}