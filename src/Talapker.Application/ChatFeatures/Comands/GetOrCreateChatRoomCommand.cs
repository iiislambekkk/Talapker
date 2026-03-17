using Microsoft.EntityFrameworkCore;
using Talapker.Infrastructure.Data;

namespace Talapker.Application.ChatFeatures.Comands;

public record GetOrCreateChatRoomCommand(string ProspectId, Guid AmbassadorId, Guid InstitutionId);

public class GetOrCreateChatRoomHandler
{
    public async Task<ChatRoomDto> Handle(
        GetOrCreateChatRoomCommand command,
        TalapkerDbContext db,
        CancellationToken ct = default)
    {
        var existing = await db.ChatRooms
            .Include(r => r.Ambassador).ThenInclude(a => a!.User)
            .FirstOrDefaultAsync(r =>
                r.ProspectId == command.ProspectId &&
                r.AmbassadorId == command.AmbassadorId &&
                r.InstitutionId == command.InstitutionId, ct);

        if (existing is not null)
            return MapToDto(existing);

        var room = new ChatRoom
        {
            Id = Guid.NewGuid(),
            ProspectId = command.ProspectId,
            AmbassadorId = command.AmbassadorId,
            InstitutionId = command.InstitutionId,
            CreatedAt = DateTime.UtcNow,
        };

        db.ChatRooms.Add(room);
        await db.SaveChangesAsync(ct);

        return MapToDto(room);
    }

    private static ChatRoomDto MapToDto(ChatRoom r) => new(
        r.Id,
        r.ProspectId,
        r.Prospect is null ? "" : r.Prospect.FirstName + " " + r.Prospect.LastName,
        r.AmbassadorId,
        r.Ambassador?.User is null ? "" : r.Ambassador.User.FirstName + " " + r.Ambassador.User.LastName,
        r.Ambassador?.AvatarUrl,
        r.InstitutionId,
        r.CreatedAt,
        r.LastMessageAt,
        null,
        null
    );
}