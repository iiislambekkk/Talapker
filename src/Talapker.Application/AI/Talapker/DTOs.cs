namespace Talapker.Application.AI.Talapker;

public record TalapkerChatRequest(
    string Message,
    Guid? UserId,
    Guid InstitutionId
);
    
public class TalapkerChatResponse
{
    public Guid UserId { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class StreamingChunk
{
    public string Content { get; set; } = string.Empty;
    public bool IsComplete { get; set; }
    public string? Error { get; set; }
}