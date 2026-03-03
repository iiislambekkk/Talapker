namespace Talapker.Notifications.Models;

public class Notification
{
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; } 
    public string? Email { get; set; }
    
    public string Type { get; set; } = "General";
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? HtmlBody { get; set; }
    
    public bool EmailSent { get; set; }
    public bool PushSent { get; set; }
    public bool IsRead { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EmailSentAt { get; set; }
    public DateTime? PushSentAt { get; set; }
    public DateTime? ReadAt { get; set; }
    
    public string? DeepLink { get; set; }
    public string? ImageUrl { get; set; }
    public Dictionary<string, string>? Data { get; set; }
}