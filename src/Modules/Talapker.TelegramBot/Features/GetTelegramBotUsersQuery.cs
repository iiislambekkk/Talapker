using Marten;
using Talapker.TelegramBot.Models;

namespace Talapker.TelegramBot.Features;

public record GetTelegramBotUsersQuery(
    Guid InstitutionId,
    int Page = 1,
    int PageSize = 20
);

public record TelegramBotUserDto(
    long TelegramUserId,
    string? TelegramUsername,
    string LastMessage,
    DateTime LastActivityAt,
    int TotalMessages
);

public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount
)
{
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

public class GetTelegramBotUsersHandler
{
    public async Task<PagedResult<TelegramBotUserDto>> Handle(
        GetTelegramBotUsersQuery query,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        var totalCount = await session.Query<TelegramBotSession>()
            .Where(s => s.InstitutionId == query.InstitutionId)
            .CountAsync(cancellationToken);

        var sessions = await session.Query<TelegramBotSession>()
            .Where(s => s.InstitutionId == query.InstitutionId)
            .OrderByDescending(s => s.LastActivityAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        var users = sessions.Select(s =>
        {
            var lastMsg = s.Messages.MaxBy(m => m.CreatedAt);
            return new TelegramBotUserDto(
                TelegramUserId: s.TelegramUserId,
                TelegramUsername: s.TelegramUsername,
                LastMessage: lastMsg?.Content ?? "",
                LastActivityAt: s.LastActivityAt,
                TotalMessages: s.Messages.Count
            );
        }).ToList();

        return new PagedResult<TelegramBotUserDto>(users, query.Page, query.PageSize, totalCount);
    }
}