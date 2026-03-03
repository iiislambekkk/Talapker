using Marten;
using Talapker.Application;
using Talapker.Notifications.Models;

namespace Talapker.Notifications.Features.Commands;

public record MarkAsReadCommand(
    Guid NotificationId,
    Guid UserId
);

public class MarkAsReadHandler
{
    public async Task<ApiResponse> Handle(MarkAsReadCommand command, IDocumentSession session)
    {
        var notification = await session.Query<Notification>()
            .FirstOrDefaultAsync(n => n.Id == command.NotificationId && 
                                      n.UserId == command.UserId);
        
        if (notification == null)
            return ApiResponse.Fail("Notification not found", ErrorCodes.Default);
        
        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        
        session.Update(notification);
        await session.SaveChangesAsync();
        
        return ApiResponse.Success();
    }
}