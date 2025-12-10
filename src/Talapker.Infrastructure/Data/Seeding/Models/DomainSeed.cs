namespace Talapker.Institutions.Infrastructure.Data.Seeding.Models;

public class DomainSeed
{
    public int DomainCode { get; set; }
    public string DomainDescriptionEn { get; set; } = string.Empty;
    public string DomainDescriptionKk { get; set; } = string.Empty;
    public string DomainDescriptionRu { get; set; } = string.Empty;
    public List<FieldSeed> FieldsOfDomain { get; set; } = new List<FieldSeed>();
}