using Microsoft.AspNetCore.SignalR;
using Talapker.Infrastructure.Data;
using Wolverine;

namespace Talapker.Application.ChatFeatures;

public class ChatHub(TalapkerDbContext db, IMessageBus messageBus) : Hub
{
    public async Task JoinRoom(Guid chatRoomId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, chatRoomId.ToString());
    }

    public async Task LeaveRoom(Guid chatRoomId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatRoomId.ToString());
    }

    public async Task SendMessage(SendMessageRequest request)
    {
        var room = await db.ChatRooms.FindAsync(request.ChatRoomId);
        if (room is null) return;

        var sender = await db.Users.FindAsync(request.SenderId);
        if (sender is null) return;

        var message = new ChatMessage
        {
            Id = Guid.NewGuid(),
            ChatRoomId = request.ChatRoomId,
            SenderId = request.SenderId,
            Text = request.Text,
            SentAt = DateTime.UtcNow,
        };

        db.ChatMessages.Add(message);
        room.LastMessageAt = message.SentAt;
        await db.SaveChangesAsync();

        var dto = new ChatMessageDto(
            message.Id,
            message.ChatRoomId,
            message.SenderId,
            sender.FirstName + " " + sender.LastName,
            message.Text,
            message.SentAt,
            message.IsRead
        );

        await Clients.Group(request.ChatRoomId.ToString())
            .SendAsync("ReceiveMessage", dto);
    }
}