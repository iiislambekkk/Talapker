namespace Talapker.Application.FacultyFeatures.Command.CreateFaculty;

public record CreateFacultyCommand(
    Guid InstitutionId,
    Dictionary<string, string> Name
);