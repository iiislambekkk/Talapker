using Talapker.Infrastructure.Data.Institution.InstitutionEntity;

namespace Talapker.Infrastructure.Data.Institution;
public class GrantCompetitionStatistic
{
    public Guid Id { get; set; }
    public int Year { get; set; }
    public GrantCompetitionType CompetitionType { get; set; }
    public Guid EducationGroupId { get; set; }
    public EducationGroup EducationGroup { get; set; } = null!;
    public List<GrantCompetitionRecord> Records { get; set; } = new List<GrantCompetitionRecord>();
    public int MinScore { get; set; }
}

public enum GrantCompetitionType
{
    General,
    Rural
}

public class GrantCompetitionRecord
{
    public int Score { get; set; }
    public int UniversityCode { get; set; }
}
