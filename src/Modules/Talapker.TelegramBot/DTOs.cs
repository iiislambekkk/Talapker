namespace Talapker.TelegramBot;

public class TelegramUserSession
{
    public Guid Id { get; set; }
    public Guid BotId { get; set; }
    public Guid InstitutionId { get; set; }
    public long TelegramUserId { get; set; }
    public string? TelegramUsername { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public UserLanguage? Language { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastActivityAt { get; set; }
    public List<TelegramSessionMessage> Messages { get; set; } = new();
}

public class TelegramSessionMessage
{
    public string Role { get; set; } = "";
    public string Content { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}

public enum UserLanguage { Kazakh, Russian, English }