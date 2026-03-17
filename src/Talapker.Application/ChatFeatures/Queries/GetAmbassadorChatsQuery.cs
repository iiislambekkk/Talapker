using Microsoft.EntityFrameworkCore;
using Talapker.Infrastructure.Data;
using Talapker.Infrastructure.Exceptions;

namespace Talapker.Application.ChatFeatures.Queries;

public record GetAmbassadorChatsQuery(Guid UserId);

public class GetAmbassadorChatsHandler
{
    public async Task<List<ChatRoomDto>> Handle(
        GetAmbassadorChatsQuery query,
        TalapkerDbContext db,
        CancellationToken ct = default)
    {
        var ambassador = await db.Ambassadors
            .FirstOrDefaultAsync(a => a.UserId == query.UserId.ToString(), ct);

        if (ambassador is null)
            throw new NotFoundException($"Ambassador not found for user {query.UserId}");

        return await db.ChatRooms
            .Where(r => r.AmbassadorId == ambassador.Id)
            .Include(r => r.Ambassador).ThenInclude(a => a!.User)
            .Include(r => r.Prospect)
            .OrderByDescending(r => r.LastMessageAt)
            .Select(r => new ChatRoomDto(
                r.Id,
                r.ProspectId,
                r.Prospect!.FirstName + " " + r.Prospect.LastName,
                r.AmbassadorId,
                r.Ambassador!.User!.FirstName + " " + r.Ambassador.User.LastName,
                r.Ambassador.AvatarUrl,
                r.InstitutionId,
                r.CreatedAt,
                r.LastMessageAt,
                r.Messages
                    .OrderByDescending(m => m.SentAt)
                    .Select(m => new ChatMessageDto(m.Id, m.ChatRoomId, m.SenderId, "", m.Text, m.SentAt, m.IsRead))
                    .FirstOrDefault(),
                r.Prospect.AvatarKey
            ))
            .ToListAsync(ct);
    }
}