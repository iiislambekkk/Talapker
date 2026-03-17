namespace LiveEvent;


public class ChatMessage
{
    public int Id { get; set; }
    public string RoomId { get; set; } = "";
    public string UserId { get; set; } = "";
    public string Username { get; set; } = "";
    public string Text { get; set; } = "";
    public string? Reaction { get; set; }   // null = normal message
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class BannedUser
{
    public int Id { get; set; }
    public string RoomId { get; set; } = "";
    public string UserId { get; set; } = "";
    public DateTime BannedAt { get; set; } = DateTime.UtcNow;
}