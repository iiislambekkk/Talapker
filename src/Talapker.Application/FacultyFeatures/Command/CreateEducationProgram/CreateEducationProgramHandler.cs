using Microsoft.EntityFrameworkCore;
using Talapker.Infrastructure;
using Talapker.Infrastructure.Data;
using Talapker.Infrastructure.Data.Institution;

namespace Talapker.Application.FacultyFeatures.Command.CreateEducationProgram;

public record CreateEducationProgramCommand(
    Guid FacultyId,
    LocalizedText Name,
    LocalizedText Description,
    LocalizedText WorkPlaces,
    LocalizedText PractiseBases,
    int MinimumUntScore,
    string Code,
    StudyForm StudyForm,
    decimal DurationYears,
    Guid EducationGroupId,
    List<Language> Languages
);

public class CreateEducationProgramHandler
{
    public async Task<ApiResponse<Guid>> Handle(
        CreateEducationProgramCommand command,
        TalapkerDbContext db,
        CancellationToken cancellationToken)
    {
        var faculty = await db.Faculties
            .FirstOrDefaultAsync(f => f.Id == command.FacultyId, cancellationToken);
            
        if (faculty == null)
            return ApiResponse<Guid>.Fail("Faculty not found", ErrorCodes.Default);
        
        var educationGroup = await db.EducationGroups
            .FirstOrDefaultAsync(g => g.Id == command.EducationGroupId, cancellationToken);
            
        if (educationGroup == null)
            return ApiResponse<Guid>.Fail("Education group not found", ErrorCodes.Default);
        
        var program = new EducationProgram
        {
            Id = Guid.NewGuid(),
            FacultyId = command.FacultyId,
            EducationGroupId = command.EducationGroupId,
            Name = command.Name,
            Description = command.Description,
            WorkPlaces = command.WorkPlaces,
            PractiseBases = command.PractiseBases,
            MinimumUntScore = command.MinimumUntScore,
            Code = command.Code,
            StudyForm = command.StudyForm,
            DurationYears = command.DurationYears,
            Languages = command.Languages ?? new List<Language>()
        };
        
        db.EducationPrograms.Add(program);
        await db.SaveChangesAsync(cancellationToken);
        
        return ApiResponse<Guid>.Success(program.Id);
    }
}