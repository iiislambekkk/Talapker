using Marten;
using Talapker.Notifications.Models;

namespace Talapker.Notifications.Features.Queries;

public record GetUserNotificationsQuery(
    Guid UserId,
    int? Limit = 50,
    bool? OnlyUnread = null
);

public class GetUserNotificationsHandler
{
    public async Task<IReadOnlyList<Notification>> Handle(GetUserNotificationsQuery query, IQuerySession session)
    {
        var notifications = session.Query<Notification>()
            .Where(n => n.UserId == query.UserId);
        
        if (query.OnlyUnread == true)
        {
            notifications = notifications.Where(n => !n.IsRead);
        }
        
        return await notifications
            .OrderByDescending(n => n.CreatedAt)
            .Take(query.Limit ?? 50)
            .ToListAsync();
    }
}