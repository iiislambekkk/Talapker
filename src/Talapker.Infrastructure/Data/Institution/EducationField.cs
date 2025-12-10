namespace Talapker.Infrastructure.Data.Institution.InstitutionEntity;

public class EducationField
{
    public Guid Id { get; set; }
    public string NationalCode { get; set; } = String.Empty;
    public LocalizedText Name { get; set; } = new LocalizedText();
    
    public Guid EducationDirectionId { get; set; }
    public EducationDirection EducationDirection { get; set; } = null!;
    
    public List<EducationGroup> EducationGroups { get; set; } = new List<EducationGroup>();
}