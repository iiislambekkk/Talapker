using Microsoft.EntityFrameworkCore;
using Talapker.Infrastructure;
using Talapker.Infrastructure.Data;
using Talapker.Infrastructure.Data.Institution;

namespace Talapker.Application.FacultyFeatures.Command.ChangeEducationProgram;

public record ChangeEducationProgramCommand(
    Guid Id,
    LocalizedText Name,
    LocalizedText Description,
    LocalizedText WorkPlaces,
    LocalizedText PractiseBases,
    int MinimumUntScore,
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

        await db.SaveChangesAsync(cancellationToken);

        return ApiResponse.Success();
    }
}