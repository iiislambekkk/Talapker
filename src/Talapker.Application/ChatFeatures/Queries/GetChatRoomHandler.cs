using Microsoft.EntityFrameworkCore;
using Talapker.Infrastructure.Data;
using Talapker.Infrastructure.Exceptions;

namespace Talapker.Application.ChatFeatures.Queries;

public record GetChatRoomQuery(Guid ChatRoomId);


public class GetChatRoomHandler
{
    public async Task<ChatRoomDto> Handle(
        GetChatRoomQuery query,
        TalapkerDbContext db,
        CancellationToken ct = default)
    {
        var room = await db.ChatRooms
            .Include(r => r.Ambassador).ThenInclude(a => a!.User)
            .Include(r => r.Prospect)
            .FirstOrDefaultAsync(r => r.Id == query.ChatRoomId, ct);

        if (room is null)
            throw new NotFoundException($"ChatRoom {query.ChatRoomId} not found");

        return new ChatRoomDto(
            room.Id,
            room.ProspectId,
            room.Prospect is null ? "" : room.Prospect.FirstName + " " + room.Prospect.LastName,
            room.AmbassadorId,
            room.Ambassador?.User is null ? "" : room.Ambassador.User.FirstName + " " + room.Ambassador.User.LastName,
            room.Ambassador?.AvatarUrl,
            room.InstitutionId,
            room.CreatedAt,
            room.LastMessageAt,
            null,
            null
        );
    }
}
