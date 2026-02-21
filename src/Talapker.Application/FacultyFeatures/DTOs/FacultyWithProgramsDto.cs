using Talapker.Infrastructure;

namespace Talapker.Application.FacultyFeatures.DTOs;

public class FacultyWithProgramsDto
{
    public Guid Id { get; set; }
    public LocalizedText Name { get; set; } = new();
    public List<EducationProgramSimpleDto> EducationPrograms { get; set; } = new();
}

public class EducationProgramSimpleDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public LocalizedText Name { get; set; } = new();
}