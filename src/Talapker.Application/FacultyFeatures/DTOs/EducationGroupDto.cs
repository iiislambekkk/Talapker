using Talapker.Infrastructure;
using Talapker.Infrastructure.Data.Institution;

namespace Talapker.Application.FacultyFeatures.DTOs;

// ─── Education Group ───────────────────────────────────────────────────────────

public class EducationGroupDto
{
    public Guid Id { get; set; }
    public string NationalCode { get; set; } = string.Empty;
    public Guid? EducationFieldId { get; set; }
    public LocalizedText Name { get; set; } = new();
    public List<UntPairDto> UntSubjectsPairs { get; set; } = new();
    public List<GrantCompetitionStatisticDto> GrantCompetitionStatistics { get; set; } = new();
}

public static class EducationGroupMapper
{
    public static EducationGroupDto ToDto(this EducationGroup entity) => new()
    {
        Id = entity.Id,
        NationalCode = entity.NationalCode,
        EducationFieldId = entity.EducationFieldId,
        Name = entity.Name,
        UntSubjectsPairs = entity.UntSubjectsPairs.Select(p => p.ToDto()).ToList(),
        GrantCompetitionStatistics = entity.GrantCompetitionStatistics.Select(g => g.ToDto()).ToList()
    };

    public static List<EducationGroupDto> ToDtoList(this IEnumerable<EducationGroup> entities) =>
        entities.Select(ToDto).ToList();
}

// ─── UNT ──────────────────────────────────────────────────────────────────────

public class UntSubjectDto
{
    public Guid Id { get; set; }
    public LocalizedText Name { get; set; } = new();
}

public class UntPairDto
{
    public Guid Id { get; set; }
    public UntSubjectDto FirstSubject { get; set; } = null!;
    public UntSubjectDto SecondSubject { get; set; } = null!;
}

public static class UntSubjectMapper
{
    public static UntSubjectDto ToDto(this UntSubject entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name
    };
}

public static class UntPairMapper
{
    public static UntPairDto ToDto(this UntPair entity) => new()
    {
        Id = entity.Id,
        FirstSubject = entity.FirstSubject!.ToDto(),
        SecondSubject = entity.SecondSubject!.ToDto()
    };
}

// ─── Grant Statistics ─────────────────────────────────────────────────────────

public class GrantCompetitionStatisticDto
{
    public Guid Id { get; set; }
    public int Year { get; set; }
    public GrantCompetitionType CompetitionType { get; set; }
    public GrantDegree Degree { get; set; }
    public int MinScore { get; set; }
    public int TotalGrants { get; set; }
    public List<FrequencyRecordDto> FrequencyRecords { get; set; } = new();
    public List<OvpoRecordDto> OvpoRecords { get; set; } = new();
}

public class FrequencyRecordDto
{
    public int Score { get; set; }
    public int Frequency { get; set; }
}

public class OvpoRecordDto
{
    public int Score { get; set; }
    public int Frequency { get; set; }
    public int Ovpo { get; set; }
}

public static class GrantCompetitionStatisticMapper
{
    public static GrantCompetitionStatisticDto ToDto(this GrantCompetitionStatistic entity) => new()
    {
        Id = entity.Id,
        Year = entity.Year,
        CompetitionType = entity.CompetitionType,
        Degree = entity.Degree,
        MinScore = entity.MinScore,
        TotalGrants = entity.TotalGrants,
        FrequencyRecords = entity.FrequencyScoreRecords.Select(r => new FrequencyRecordDto
        {
            Score = r.Score,
            Frequency = r.Frequency
        }).ToList(),
        OvpoRecords = entity.OvpoScoreRecords.Select(r => new OvpoRecordDto
        {
            Score = r.Score,
            Frequency = r.Frequency,
            Ovpo = r.Ovpo
        }).ToList()
    };
}