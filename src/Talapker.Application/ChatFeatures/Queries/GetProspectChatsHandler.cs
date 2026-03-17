using Microsoft.EntityFrameworkCore;
using Talapker.Infrastructure.Data;

namespace Talapker.Application.ChatFeatures.Queries;

public record GetProspectChatsQuery(string ProspectId, Guid InstitutionId);

public class GetProspectChatsHandler
{
    public async Task<List<ChatRoomDto>> Handle(
        GetProspectChatsQuery query,
        TalapkerDbContext db,
        CancellationToken ct = default)
    {
        return await db.ChatRooms
            .Where(r => r.ProspectId == query.ProspectId && r.InstitutionId == query.InstitutionId)
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
                null
            ))
            .ToListAsync(ct);
    }
}