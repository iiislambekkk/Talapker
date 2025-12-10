namespace Talapker.Institutions.Infrastructure.Data.Seeding.Models;

public class ClassificationSeed
{
    public int Id { get; set; }
    public string DegreeEn { get; set; } = string.Empty;
    public string DegreeKk { get; set; } = string.Empty;
    public string DegreeRu { get; set; } = string.Empty;
    public List<DomainSeed> Domains { get; set; } = new List<DomainSeed>();
}