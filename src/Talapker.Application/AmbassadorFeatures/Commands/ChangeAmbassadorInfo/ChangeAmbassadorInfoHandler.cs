using Microsoft.EntityFrameworkCore;
using Talapker.Infrastructure.Data;

namespace Talapker.Application.AmbassadorFeatures.Commands.ChangeAmbassadorInfo;

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

        if (String.IsNullOrEmpty(ambassador.FullName))
        {
            ambassador.FullName = "NoName";
        }
        ambassador.FullName = command.FullName;
        ambassador.AvatarUrl = command.AvatarUrl;
        ambassador.WallPaperUrl = command.WallPaperUrl;
        ambassador.EducationalProgramId = command.EducationalProgramId;
        ambassador.StudyYear = command.StudyYear;
        ambassador.DegreeType = command.DegreeType;
        ambassador.Tagline = command.Tagline;
        ambassador.Bio = command.Bio;
        ambassador.Languages = command.Languages;
        ambassador.Interests = command.Interests;
        ambassador.SocialLinks = command.SocialLinks;
        ambassador.HasCompletedOnboarding = true;

        await db.SaveChangesAsync(cancellationToken);

        return ApiResponse.Success();
    }
}