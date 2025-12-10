namespace Talapker.Institutions.Infrastructure.Data.Seeding.Models;

public class GrantCompetitionStatisticsSeed
{
    public string CompetitionType { get; set; } = String.Empty;
    public List<Record> Records { get; set; } = new List<Record>();
}

public record Record (int Score, int Ovpo);