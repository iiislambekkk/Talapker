using Talapker.Infrastructure.Data.UserAccess;

namespace Talapker.Infrastructure.Data.Assistant;

public class AssistantChatMessage
{
    public Guid Id { get; set; }
        
    // Кто отправил
    public Guid? UserId { get; set; } // null для анонимов
    public string SessionId { get; set; } = string.Empty; // для неавторизованных
    
    public Guid InstitutionId { get; set; }
    public Institution.Institution Institution { get; set; }
        
    // Само сообщение
    public string Role { get; set; } = string.Empty; // "user" или "assistant"
    public string Content { get; set; } = string.Empty;
        
    // Временные метки
    public DateTime CreatedAt { get; set; }
        
    // Метаданные
    public string? ToolCalls { get; set; } // JSON с вызовами инструментов если были
        
    // Навигационное свойство
    public ApplicationUser? User { get; set; }
}