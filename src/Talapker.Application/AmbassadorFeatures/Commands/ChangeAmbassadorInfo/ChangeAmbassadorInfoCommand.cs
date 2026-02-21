namespace Talapker.Application.AmbassadorFeatures.Commands.ChangeAmbassadorInfo;

public record ChangeAmbassadorInfoCommand(
    Guid Id,
    string FullName,
    string? AvatarUrl,
    string? WallPaperUrl,
    Guid? EducationalProgramId,
    int StudyYear,
    string? DegreeType,
    string? Tagline,
    string? Bio,
    List<string> Languages,
    List<string> Interests,
    Dictionary<string, string>? SocialLinks
);