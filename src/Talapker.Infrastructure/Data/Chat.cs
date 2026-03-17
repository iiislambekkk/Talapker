using Talapker.Infrastructure.Data.Institution;
using Talapker.Infrastructure.Data.UserAccess;

namespace Talapker.Infrastructure.Data;

public class ChatRoom
{
    public Guid Id { get; set; }
    
    public string ProspectId { get; set; } = string.Empty;
    public ApplicationUser? Prospect { get; set; }
    
    public Guid AmbassadorId { get; set; }
    public Ambassador? Ambassador { get; set; }
    
    public Guid InstitutionId { get; set; }
    public Institution.Institution? Institution { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastMessageAt { get; set; }
    
    public List<ChatMessage> Messages { get; set; } = new();
}

public class ChatMessage
{
    public Guid Id { get; set; }
    
    public Guid ChatRoomId { get; set; }
    public ChatRoom? ChatRoom { get; set; }
    
    public string SenderId { get; set; } = string.Empty;
    public ApplicationUser? Sender { get; set; }
    
    public string Text { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; }
}