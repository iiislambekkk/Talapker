namespace Talapker.Institutions.Infrastructure.Data.Seeding.Models;

public class UntPairSeed
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public int FirstSubject { get; set; }
    public int SecondSubject { get; set; }
}