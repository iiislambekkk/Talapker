namespace Talapker.Infrastructure.Data.Institution.InstitutionEntity;

public class EducationProgram
{
    public Guid Id { get; set; }
    
    public LocalizedText Name { get; set; } = new LocalizedText();
    public LocalizedText Description { get; set; } = new LocalizedText();
    
    public Guid FacultyId { get; set; }
    public Faculty Faculty { get; set; } = null!;
    
    public Guid EducationGroupId { get; set; }
    public EducationGroup? EducationGroup { get; set; }
}