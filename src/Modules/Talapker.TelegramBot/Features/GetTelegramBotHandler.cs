using Marten;

namespace Talapker.TelegramBot.Features;

public record GetTelegramBotQuery(Guid InstitutionId);

public class GetTelegramBotDto
{
    public Guid Id { get; set; }
    public string BotUsername { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class GetTelegramBotHandler
{
    public async Task<GetTelegramBotDto?> Handle(
        GetTelegramBotQuery query,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        var bot = await session.Query<Models.TelegramBot>()
            .FirstOrDefaultAsync(b => b.InstitutionId == query.InstitutionId && b.IsActive, cancellationToken);

        if (bot is null)
            return null;

        return new GetTelegramBotDto
        {
            Id = bot.Id,
            BotUsername = bot.BotUsername,
            IsActive = bot.IsActive,
            CreatedAt = bot.CreatedAt,
        };
    }
}