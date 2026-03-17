using Microsoft.EntityFrameworkCore;
using Talapker.Infrastructure.Data;

namespace Talapker.Application.AmbassadorFeatures.Commands;

public record ChangeAmbassadorInfoCommand(
    Guid Id,
    string? AvatarUrl,
    Guid? EducationalProgramId,
    int StudyYear,
    string? DegreeType,
    string? Tagline,
    string? Bio,
    List<string> Languages,
    List<string> Interests,
    Dictionary<string, string>? SocialLinks
);

public class ChangeAmbassadorInfoHandler()
{
    public async Task<ApiResponse> Handle(
        ChangeAmbassadorInfoCommand command,
        TalapkerDbContext db,
        CancellationToken cancellationToken)
    {
        var ambassador = await db.Ambassadors
            .Include(a => a.EducationProgram)
            .FirstOrDefaultAsync(a => a.Id == command.Id, cancellationToken);

        if (ambassador == null)
            return ApiResponse.Fail("Ambassador not found", ErrorCodes.Default);

        if (command.EducationalProgramId.HasValue)
        {
            var program = await db.EducationPrograms
                .FirstOrDefaultAsync(p => p.Id == command.EducationalProgramId, cancellationToken);

            if (program == null)
                return ApiResponse.Fail("Educational program not found", ErrorCodes.Default);
        }

        ambassador.AvatarUrl = command.AvatarUrl;
        ambassador.EducationalProgramId = command.EducationalProgramId;
        ambassador.StudyYear = command.StudyYear;
        ambassador.DegreeType = command.DegreeType;
        ambassador.Tagline = command.Tagline;
        ambassador.Bio = command.Bio;
        ambassador.Languages = command.Languages;
        ambassador.Interests = command.Interests;
        ambassador.SocialLinks = command.SocialLinks;

        await db.SaveChangesAsync(cancellationToken);

        return ApiResponse.Success();
    }
}