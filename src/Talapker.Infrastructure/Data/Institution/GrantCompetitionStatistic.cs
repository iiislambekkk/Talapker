using Talapker.Infrastructure.Data.Institution.InstitutionEntity;

namespace Talapker.Infrastructure.Data.Institution;
public class GrantCompetitionStatistic
{
    public Guid Id { get; set; }
    public int Year { get; set; }
    public GrantCompetitionType CompetitionType { get; set; }
    public Guid EducationGroupId { get; set; }
    public EducationGroup EducationGroup { get; set; } = null!;
    public List<GrantCompetitionFrequencyRecord> FrequencyScoreRecords { get; set; } = new List<GrantCompetitionFrequencyRecord>();
    public List<GrantCompetitionOvpoRecord> OvpoScoreRecords { get; set; } = new List<GrantCompetitionOvpoRecord>();
    public int MinScore { get; set; }
    public int TotalGrants { get; set; }
    public GrantDegree Degree { get; set; }
}

public enum GrantCompetitionType
{
    General,
    Rural,
    Profile,
    Ped
}

public enum GrantDegree
{
    Bachelor,
    Magistracy,
    Philosopher
}

public class GrantCompetitionFrequencyRecord
{
    public int Score { get; set; }
    public int Frequency  { get; set; }
}

public class GrantCompetitionOvpoRecord
{
    public int Score { get; set; }
    public int Frequency  { get; set; }
    public int Ovpo { get; set; }
}
