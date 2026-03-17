namespace Talapker.TelegramBot.Models;

public class TelegramBotSession
{
    public Guid Id { get; set; }
    public Guid BotId { get; set; }
    public Guid InstitutionId { get; set; }

    public long TelegramUserId { get; set; }
    public string? TelegramUsername { get; set; }
    public List<TelegramSessionMessage> Messages { get; set; } = new();
    public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;
}