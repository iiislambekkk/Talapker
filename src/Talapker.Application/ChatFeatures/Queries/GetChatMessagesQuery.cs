using Microsoft.EntityFrameworkCore;
using Talapker.Infrastructure.Data;

namespace Talapker.Application.ChatFeatures.Queries;

public record GetChatMessagesQuery(Guid ChatRoomId, int Page = 1, int PageSize = 50);

public class GetChatMessagesHandler
{
    public async Task<List<ChatMessageDto>> Handle(
        GetChatMessagesQuery query,
        TalapkerDbContext db,
        CancellationToken ct = default)
    {
        var messages = await db.ChatMessages
            .Where(m => m.ChatRoomId == query.ChatRoomId)
            .OrderByDescending(m => m.SentAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(m => new ChatMessageDto(
                m.Id,
                m.ChatRoomId,
                m.SenderId,
                m.Sender!.FirstName + " " + m.Sender.LastName,
                m.Text,
                m.SentAt,
                m.IsRead
            ))
            .ToListAsync(ct);

        messages.Sort((a, b) => a.SentAt.CompareTo(b.SentAt));
        return messages;
    }
}