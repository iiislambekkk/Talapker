namespace Talapker.Infrastructure.Data.Institution;

public class Ambassador
{
    public Guid Id { get; set; }
    public bool HasCompletedOnboarding { get; set; }
    
    public string FullName { get; set; }
    public string Email { get; set; }
    public string? AvatarUrl { get; set; }
    public string? WallPaperUrl { get; set; }
    
    public Guid? InstitutionId { get; set; }
    public Institution? Institution { get; set; }
    
    public Guid? EducationalProgramId { get; set; }
    public EducationProgram? EducationProgram { get; set; }
    
    public int StudyYear { get; set; }
    public string? DegreeType { get; set; }
    
    public string? Tagline { get; set; }
    public string? Bio { get; set; }
    public List<string> Languages { get; set; } = new();
    public List<string> Interests { get; set; } = new();
    public Dictionary<string, string>? SocialLinks { get; set; }
    
    public int TotalChats { get; set; }
    public int TotalReplies { get; set; }
    public double AverageResponseTime { get; set; }
    public int HelpfulVotes { get; set; }
    public double Rating { get; set; }
    
    public bool IsActive { get; set; }
    public DateTime DateJoined { get; set; }
    public DateTime? LastActiveAt { get; set; }
}

