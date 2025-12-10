namespace Talapker.Institutions.Infrastructure.Data.Seeding.Models;

public class GroupSeed
{
    public string Code { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public string DescriptionKk { get; set; } = string.Empty;
    public string DescriptionRu { get; set; } = string.Empty;
    public List<int> SubjectPairsCodes { get; set; } = new List<int>();
}