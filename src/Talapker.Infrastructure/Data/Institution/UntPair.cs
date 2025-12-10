namespace Talapker.Infrastructure.Data.Institution.InstitutionEntity;

public class UntPair
{
    public Guid Id { get; set; }
    public int? SeedId { get; set; }
    
    public Guid FirstSubjectId { get; set; }
    public Guid SecondSubjectId { get; set; }
    
    public UntSubject? FirstSubject { get; set; }
    public UntSubject? SecondSubject { get; set; }
    
    public List<EducationGroup> EducationGroups { get; set; } = new List<EducationGroup>();
}

