namespace Talapker.Infrastructure.Data.Tg;

public class BotToken
{
    public Guid BotId { get; set; }
    public string EncryptedToken { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}