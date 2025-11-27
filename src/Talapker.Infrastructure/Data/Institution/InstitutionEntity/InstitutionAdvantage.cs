namespace Talapker.Infrastructure.Data.Institution.InstitutionEntity;

public class InstitutionAdvantage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public LocalizedText Title { get; set; }  = new();
    public LocalizedText Description { get; set; }  = new();
} 