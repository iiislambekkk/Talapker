namespace Talapker.Institutions.Infrastructure.Data.Seeding.Models;

public class UniversitySeed
{
    public string NameKk { get; set; } = null!;
    public string NameRu { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    
    public int Code { get; set; }
    public int CityId { get; set; }
    public string Website { get; set; } = String.Empty;
}