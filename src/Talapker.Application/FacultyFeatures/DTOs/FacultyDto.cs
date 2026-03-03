using Talapker.Application.FacultyFeatures.DTOs.Mappers;
using Talapker.Application.FacultyFeatures.Queries.GetEducationProgramsQuery;
using Talapker.Infrastructure;

namespace Talapker.Application.FacultyFeatures.DTOs;

public class FacultyDto
{
    public Guid Id { get; set; }
    public LocalizedText Name { get; set; } = new();
    public Guid InstitutionId { get; set; }
    public string? LogoUrl { get; set; }
    public string? WallPaperUrl { get; set; }
    public List<EducationProgramDto> EducationPrograms { get; set; } = new();
}

public class FacultySlimDto
{
    public Guid Id { get; set; }
    public LocalizedText Name { get; set; } = new();
    public Guid InstitutionId { get; set; }
    public string? LogoUrl { get; set; }
    public string? WallPaperUrl { get; set; }
}
