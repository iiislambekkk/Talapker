namespace Talapker.Application.FacultyFeatures.Command.ChangeFaculty;

public record ChangeFacultyCommand(
    Guid Id,
    Dictionary<string, string> Name,
    string? LogoUrl,
    string? WallPaperUrl
);