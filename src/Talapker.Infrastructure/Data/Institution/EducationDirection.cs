using Talapker.Infrastructure.Data.Institution.InstitutionEntity;

namespace Talapker.Infrastructure.Data.Institution;

public enum Degree
{
    Bachelor,
    Magistracy,
    Doctor
}

public class EducationDirection
{
    public Guid Id { get; set; }
      
    public LocalizedText Name { get; set; } = new();
    
    public Degree Degree { get; set; }
    
    public List<EducationField> EducationFields { get; set; } = new List<EducationField>();
}