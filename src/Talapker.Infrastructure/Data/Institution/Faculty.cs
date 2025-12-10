namespace Talapker.Infrastructure.Data.Institution.InstitutionEntity;

public class Faculty
{
    public Guid Id { get; set; }
    public LocalizedText Name { get; set; } = new LocalizedText();

    public Guid InstitutionId { get; set; }
    public Institution? Institution { get; set; }
    public List<EducationProgram> EducationPrograms { get; set; } = new List<EducationProgram>();
}