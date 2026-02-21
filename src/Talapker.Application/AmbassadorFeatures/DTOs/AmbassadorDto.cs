using Talapker.Application.InstitutionFeatures.DTOs;
using Talapker.Infrastructure.Data.Institution;

namespace Talapker.Application.AmbassadorFeatures.DTOs;

public class AmbassadorDto
{
    public Guid Id { get; set; }
    public bool HasCompletedOnboarding { get; set; }
    
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? WallPaperUrl { get; set; }
    
    public Guid? InstitutionId { get; set; }
    public InstitutionDto? Institution { get; set; }
    
    public Guid? EducationalProgramId { get; set; }
    public string? EducationalProgramName { get; set; }
    
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

public static class AmbassadorMapper
{
    public static AmbassadorDto ToDto(this Ambassador ambassador)
    {
        return new AmbassadorDto
        {
            Id = ambassador.Id,
            HasCompletedOnboarding = ambassador.HasCompletedOnboarding,
            FullName = ambassador.FullName,
            Email = ambassador.Email,
            AvatarUrl = ambassador.AvatarUrl,
            WallPaperUrl = ambassador.WallPaperUrl,
            InstitutionId = ambassador.InstitutionId,
            EducationalProgramId = ambassador.EducationalProgramId,
            EducationalProgramName = ambassador.EducationProgram?.Name.Ru ?? "",
            StudyYear = ambassador.StudyYear,
            DegreeType = ambassador.DegreeType,
            Tagline = ambassador.Tagline,
            Bio = ambassador.Bio,
            Languages = ambassador.Languages,
            Interests = ambassador.Interests,
            SocialLinks = ambassador.SocialLinks,
            TotalChats = ambassador.TotalChats,
            TotalReplies = ambassador.TotalReplies,
            AverageResponseTime = ambassador.AverageResponseTime,
            HelpfulVotes = ambassador.HelpfulVotes,
            Rating = ambassador.Rating,
            IsActive = ambassador.IsActive,
            DateJoined = ambassador.DateJoined,
            LastActiveAt = ambassador.LastActiveAt
        };
    }
}