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

    public Guid? FacultyId { get; set; }
    public LocalizedText FacultyName { get; set; } = new();

    public Guid? EducationGroupId { get; set; }
    public LocalizedText EducationGroupName { get; set; } = new();
    public string EducationGroupCode { get; set; } = string.Empty;

    public List<Language> Languages { get; set; } = new();
    public List<EducationProgramPriceDto> Prices { get; set; } = new();
    public List<EducationProgramDisciplineDto> Disciplines { get; set; } = new();
}

public static class EducationProgramMapper
{
    public static EducationProgramDto ToDto(this EducationProgram program)
    {
        return new EducationProgramDto
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
            FacultyId = program.FacultyId,
            FacultyName = program.Faculty?.Name ?? new LocalizedText(),
            EducationGroupId = program.EducationGroupId,
            EducationGroupName = program.EducationGroup?.Name ?? new LocalizedText(),
            EducationGroupCode = program.EducationGroup?.NationalCode ?? "",
            Languages = program.Languages ?? new List<Language>(),
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
    }

    public static List<EducationProgramDto> ToDtoList(this IEnumerable<EducationProgram> programs)
    {
        return programs.Select(p => p.ToDto()).ToList();
    }
}