namespace Talapker.TelegramBot.Models;

public class TelegramBot
{
    public Guid Id { get; set; }
    public Guid InstitutionId { get; set; }

    public string BotToken { get; set; } = string.Empty;
    public string BotUsername { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}