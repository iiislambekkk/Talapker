namespace Talapker.Infrastructure.Data.Institution.InstitutionEntity;

public class EducationGroup
{
    public Guid Id { get; set; }
    public string NationalCode { get; set; } = String.Empty;
    public Guid EducationFieldId { get; set; }
    public EducationField EducationField { get; set; } = null!;
    public LocalizedText Name { get; set; } = new LocalizedText();
    public List<UntPair> UntSubjectsPairs { get; set; } = new List<UntPair>();
    public List<GrantCompetitionStatistic> GrantCompetitionStatistics { get; set; } = new ();
    
    public List<EducationProgram> EducationPrograms { get; set; } = new List<EducationProgram>();
}