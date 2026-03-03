using Marten;
using Talapker.Notifications.Models;

namespace Talapker.Notifications.Features.Queries;

public record GetNotificationQuery(
    Guid NotificationId,
    Guid UserId
);

public class GetNotificationHandler
{
    public async Task<Notification?> Handle(GetNotificationQuery query, IQuerySession session)
    {
        return await session.Query<Notification>()
            .FirstOrDefaultAsync(n => n.Id == query.NotificationId && 
                                      n.UserId == query.UserId);
    }
}