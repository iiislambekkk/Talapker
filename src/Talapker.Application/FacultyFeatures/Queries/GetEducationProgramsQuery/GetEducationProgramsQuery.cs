namespace Talapker.Application.FacultyFeatures.Queries.GetEducationProgramsQuery;

public record GetEducationProgramsQuery(
    Guid InstitutionId,
    Guid? FacultyId = null
);