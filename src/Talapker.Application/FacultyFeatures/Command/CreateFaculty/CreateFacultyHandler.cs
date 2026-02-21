using Microsoft.EntityFrameworkCore;
using Talapker.Infrastructure;
using Talapker.Infrastructure.Data;
using Talapker.Infrastructure.Data.Institution;

namespace Talapker.Application.FacultyFeatures.Command.CreateFaculty;

public class CreateFacultyHandler()
{
    public async Task<ApiResponse<Guid>> Handle(CreateFacultyCommand command, TalapkerDbContext db, CancellationToken cancellationToken)
    {
        var institution = await db.Institutions
            .FirstOrDefaultAsync(i => i.Id == command.InstitutionId, cancellationToken);
            
        if (institution == null)
            return ApiResponse<Guid>.Fail("Institution not found", ErrorCodes.Default);
        
        var faculty = new Faculty
        {
            Id = Guid.NewGuid(),
            InstitutionId = command.InstitutionId,
            Name = new LocalizedText
            {
                Kk = command.Name.GetValueOrDefault("kk", ""),
                Ru = command.Name.GetValueOrDefault("ru", ""),
                En = command.Name.GetValueOrDefault("en", "")
            }
        };
        
        db.Faculties.Add(faculty);
        await db.SaveChangesAsync(cancellationToken);
        
        return ApiResponse<Guid>.Success(faculty.Id);
    }
}