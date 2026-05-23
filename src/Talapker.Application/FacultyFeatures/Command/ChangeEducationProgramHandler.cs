using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Talapker.Application.AI.Talapker;
using Talapker.Infrastructure;
using Talapker.Infrastructure.Data;
using Talapker.Infrastructure.Data.Institution;

namespace Talapker.Application.FacultyFeatures.Command;

public record ChangeEducationProgramCommand(
    Guid Id,
    LocalizedText Name,
    LocalizedText Description,
    LocalizedText WorkPlaces,
    LocalizedText PractiseBases,
    int MinimumUntScore,
    int MinimumGrantUntScore,
    int MinimumPlatnoeUntScore,
    string Code,
    StudyForm StudyForm,
    decimal DurationYears,
    Guid EducationGroupId,
    List<Language> Languages,
    Guid FacultyId
);

public class ChangeEducationProgramHandler
{
    public async Task<ApiResponse> Handle(
        ChangeEducationProgramCommand command,
        TalapkerDbContext db,
        IDistributedCache cache,
        CancellationToken cancellationToken)
    {
        var program = await db.EducationPrograms
            .FirstOrDefaultAsync(p => p.Id == command.Id, cancellationToken);

        if (program == null)
            return ApiResponse.Fail("Education program not found", ErrorCodes.Default);

        var educationGroup = await db.EducationGroups
            .FirstOrDefaultAsync(g => g.Id == command.EducationGroupId, cancellationToken);

        if (educationGroup == null)
            return ApiResponse.Fail("Education group not found", ErrorCodes.Default);
        
        var faculty = await db.Faculties
            .FirstOrDefaultAsync(f => f.Id == command.FacultyId, cancellationToken);
            
        if (faculty == null)
            return ApiResponse.Fail("Faculty not found", ErrorCodes.Default);

        program.Name = command.Name;
        program.Description = command.Description;
        program.WorkPlaces = command.WorkPlaces;
        program.PractiseBases = command.PractiseBases;
        program.MinimumUntScore = command.MinimumUntScore;
        program.Code = command.Code;
        program.StudyForm = command.StudyForm;
        program.DurationYears = command.DurationYears;
        program.EducationGroupId = command.EducationGroupId;
        program.Languages = command.Languages ?? new List<Language>();
        program.FacultyId = command.FacultyId;
        program.MinimumGrantUntScore =  command.MinimumGrantUntScore;
        program.MinimumPlatnoeUntScore =  command.MinimumPlatnoeUntScore;

        await db.SaveChangesAsync(cancellationToken);
        await TalapkerToolsCache.InvalidateAllProgramsAsync(cache, cancellationToken);
        await TalapkerToolsCache.InvalidateInstitutionContextAsync(cache, faculty.InstitutionId, cancellationToken);

        return ApiResponse.Success();
    }
}