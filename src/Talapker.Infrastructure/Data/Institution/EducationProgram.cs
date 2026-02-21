using Talapker.Infrastructure.Data.Institution.InstitutionEntity;

namespace Talapker.Infrastructure.Data.Institution;

public enum Language
{
    Kazakh,
    Russian,
    English
}

public enum StudyForm
{
    FullTime,   
    PartTime,  
    Evening,   
    Distance
}

public class EducationProgramDiscipline
{
    public LocalizedText Name { get; set; } = new();
    public LocalizedText Description { get; set; } = new();
    public int Credits { get; set; }
    public List<int> Semesters { get; set; } = new();
}

public class EducationProgramPrice
{
    public int Year { get; set; }
    public decimal Amount { get; set; }
    public StudyForm StudyForm { get; set; }
}


public class EducationProgram
{
    public Guid Id { get; set; }

    public LocalizedText Name { get; set; } = new();
    public LocalizedText Description { get; set; } = new();
    public LocalizedText WorkPlaces { get; set; } = new();
    public LocalizedText PractiseBases { get; set; } = new();

    public int MinimumUntScore { get; set; }
    public string Code { get; set; } = string.Empty;

    public StudyForm StudyForm { get; set; } = StudyForm.FullTime;
    public decimal DurationYears { get; set; }

    public List<Language> Languages { get; set; } = new();
    public List<EducationProgramPrice> Prices { get; set; } = new();
    public List<EducationProgramDiscipline> Disciplines { get; set; } = new();

    public Guid? FacultyId { get; set; }
    public Faculty? Faculty { get; set; }

    public Guid? EducationGroupId { get; set; }
    public EducationGroup? EducationGroup { get; set; }
}

