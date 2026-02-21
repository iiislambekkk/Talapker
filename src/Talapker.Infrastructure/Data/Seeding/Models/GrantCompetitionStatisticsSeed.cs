namespace Talapker.Infrastructure.Data.Seeding.Models;

public class GrantCompetitionStatisticsSeed
{
    public string CompetitionType { get; set; } = string.Empty;
    public List<RawRecord> Records { get; set; } = new();
}

public record RawRecord(int Num, string Ikt, int Score, int Ovpo);