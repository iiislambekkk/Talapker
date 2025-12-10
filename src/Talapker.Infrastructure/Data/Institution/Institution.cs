namespace Talapker.Infrastructure.Data.Institution.InstitutionEntity;

public class Institution
{
    public Guid Id { get; set; }
    
    public int? NationalCode { get; set; }
    public LocalizedText Name { get; set; } = new();
    public LocalizedText Description { get; set; } = new();
    public InstitutionType  Type { get; set; }
    
    public int StudentsCount { get; set; }
    public long MinCostPerYear { get; set; }
    
    public bool? HasHousing { get; set; }
    public bool? HasMilitaryDepartment { get; set; }
    public string LogoKey { get; set; } = String.Empty;
    public string WebSiteUrl { get; set; } = string.Empty;
    public string Coordinates { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    
    public List<InstitutionAdvantage> Advantages { get; set; } = new();
    
    public Guid? CityId { get; set; }
    public City? City { get; set; }
    public List<Faculty> Faculties { get; set; } = new();
    
    // public List<GrantCompetitionStatistic> GrantCompetitionStatistics { get; set; } = new ();
}