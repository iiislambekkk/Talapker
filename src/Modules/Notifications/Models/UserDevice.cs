namespace Talapker.Notifications.Models;

public class UserDevice
{
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; } 
    public string DeviceToken { get; set; } = string.Empty; // Firebase token
    
    public string? Platform { get; set; } // "android", "ios", "web"
    public string? DeviceModel { get; set; }
    public string? AppVersion { get; set; }
    
    public bool IsActive { get; set; } = true;
    public DateTime LastUsedAt { get; set; } = DateTime.UtcNow;
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? ExpiresAt { get; set; }
}