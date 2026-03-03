using Talapker.Application.FacultyFeatures.Queries.GetEducationProgramsQuery;
using Talapker.Infrastructure;
using Talapker.Infrastructure.Data.Institution;

namespace Talapker.Application.FacultyFeatures.DTOs.Mappers;

public class EducationProgramPriceDto
{
    public int Year { get; set; }
    public decimal Amount { get; set; }
    public StudyForm StudyForm { get; set; }
}

public class EducationProgramDisciplineDto
{
    public LocalizedText Name { get; set; } = new();
    public LocalizedText Description { get; set; } = new();
    public int Credits { get; set; }
    public List<int> Semesters { get; set; } = new();
}

public class EducationProgramDto
{
    public Guid Id { get; set; }
    public LocalizedText Name { get; set; } = new();
    public LocalizedText Description { get; set; } = new();
    public LocalizedText WorkPlaces { get; set; } = new();
    public LocalizedText PractiseBases { get; set; } = new();

    public int MinimumUntScore { get; set; }
    public string Code { get; set; } = string.Empty;
    public StudyForm StudyForm { get; set; }
    public decimal DurationYears { get; set; }
    public List<Language> Languages { get; set; } = new();

    public FacultySlimDto? Faculty { get; set; } = new();

    public EducationGroupDto? EducationGroup { get; set; }

    public List<EducationProgramPriceDto> Prices { get; set; } = new();
    public List<EducationProgramDisciplineDto> Disciplines { get; set; } = new();
}


public static class EducationProgramMapper
{
    public static EducationProgramDto ToDto(this EducationProgram program) => new()
    {
        Id = program.Id,
        Name = program.Name,
        Description = program.Description,
        WorkPlaces = program.WorkPlaces,
        PractiseBases = program.PractiseBases,
        MinimumUntScore = program.MinimumUntScore,
        Code = program.Code,
        StudyForm = program.StudyForm,
        DurationYears = program.DurationYears,
        Languages = program.Languages ?? new(),

        Faculty = program.Faculty?.ToSlimDto() ?? new(),

        EducationGroup = program.EducationGroup?.ToDto(),

        Prices = program.Prices
            .OrderBy(p => p.Year)
            .Select(p => new EducationProgramPriceDto
            {
                Year = p.Year,
                Amount = p.Amount,
                StudyForm = p.StudyForm
            })
            .ToList(),

        Disciplines = program.Disciplines
            .OrderBy(d => d.Semesters.Min())
            .Select(d => new EducationProgramDisciplineDto
            {
                Name = d.Name,
                Description = d.Description,
                Credits = d.Credits,
                Semesters = d.Semesters
            })
            .ToList()
    };
    
    public static List<EducationProgramDto> ToDtoList(this IEnumerable<EducationProgram> programs) =>
        programs.Select(p => p.ToDto()).ToList();
}

