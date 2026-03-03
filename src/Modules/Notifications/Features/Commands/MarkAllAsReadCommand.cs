using Marten;
using Talapker.Application;
using Talapker.Notifications.Models;

namespace Talapker.Notifications.Features.Commands;

public record MarkAllAsReadCommand(
    Guid UserId
);

public class MarkAllAsReadHandler
{
    public async Task<ApiResponse> Handle(MarkAllAsReadCommand command, IDocumentSession session)
    {
        var notifications = await session.Query<Notification>()
            .Where(n => n.UserId == command.UserId && !n.IsRead)
            .ToListAsync();
        
        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
        }
        
        session.Update(notifications.ToArray());
        await session.SaveChangesAsync();
        
        return ApiResponse.Success();
    }
}