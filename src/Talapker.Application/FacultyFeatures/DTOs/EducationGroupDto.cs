using Talapker.Infrastructure;
using Talapker.Infrastructure.Data.Institution;

namespace Talapker.Application.FacultyFeatures.DTOs;

public class EducationGroupDto
{
    public Guid Id { get; set; }
    public string NationalCode { get; set; } = string.Empty;
    public Guid EducationFieldId { get; set; }
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

public class UntPairDto
{
    public Guid Id { get; set; }
    public UntSubjectDto FirstSubject { get; set; } = null!;
    public UntSubjectDto SecondSubject { get; set; } = null!;
}

public class UntSubjectDto
{
    public Guid Id { get; set; }
    public LocalizedText Name { get; set; } = new();
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

public static class UntSubjectMapper
{
    public static UntSubjectDto ToDto(this UntSubject entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name
    };
}

public class GrantCompetitionStatisticDto
{
    public Guid Id { get; set; }
    public int Year { get; set; }
    public GrantCompetitionType CompetitionType { get; set; }
    public int MinScore { get; set; }
    public int TotalGrants { get; set; }
    public List<GrantCompetitionRecordDto> Records { get; set; } = new();
}

public class GrantCompetitionRecordDto
{
    public int Score { get; set; }
    public int Frequency { get; set; }
}

public static class GrantCompetitionStatisticMapper
{
    public static GrantCompetitionStatisticDto ToDto(this GrantCompetitionStatistic entity) => new()
    {
        Id = entity.Id,
        Year = entity.Year,
        CompetitionType = entity.CompetitionType,
        MinScore = entity.MinScore,
        TotalGrants = entity.TotalGrants,
        Records = entity.Records.Select(r => r.ToDto()).ToList()
    };
}

public static class GrantCompetitionRecordMapper
{
    public static GrantCompetitionRecordDto ToDto(this GrantCompetitionRecord entity) => new()
    {
        Score = entity.Score,
        Frequency = entity.Frequency
    };
}