namespace Talapker.Infrastructure.Data.Institution;

public class City
{
    public Guid Id { get; set; }
    public string LogoKey { get; set; } = String.Empty;
    public LocalizedText Name { get; set; }
    public List<InstitutionEntity.Institution> Institutions { get; set; } = new ();
}