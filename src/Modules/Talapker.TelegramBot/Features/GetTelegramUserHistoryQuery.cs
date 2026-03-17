using Marten;
using Talapker.TelegramBot.Models;

namespace Talapker.TelegramBot.Features;

public record GetTelegramUserHistoryQuery(
    Guid InstitutionId,
    long TelegramUserId
);

public record TelegramUserHistoryDto(
    long TelegramUserId,
    string? TelegramUsername,
    DateTime LastActivityAt,
    IReadOnlyList<TelegramSessionMessageDto> Messages
);

public record TelegramSessionMessageDto(
    string Role,
    string Content,
    DateTime CreatedAt
);

public class GetTelegramUserHistoryHandler
{
    public async Task<TelegramUserHistoryDto?> Handle(
        GetTelegramUserHistoryQuery query,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        var botSession = await session.Query<TelegramBotSession>()
            .FirstOrDefaultAsync(
                s => s.InstitutionId == query.InstitutionId &&
                     s.TelegramUserId == query.TelegramUserId,
                cancellationToken);

        if (botSession is null)
            return null;

        var messages = botSession.Messages
            .OrderBy(m => m.CreatedAt)
            .Select(m => new TelegramSessionMessageDto(m.Role, m.Content, m.CreatedAt))
            .ToList();

        return new TelegramUserHistoryDto(
            TelegramUserId: botSession.TelegramUserId,
            TelegramUsername: botSession.TelegramUsername,
            LastActivityAt: botSession.LastActivityAt,
            Messages: messages
        );
    }
}