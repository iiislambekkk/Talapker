using Marten;
using Talapker.Application;
using Talapker.Infrastructure.Exceptions;
using Talapker.Notifications.Contracts;
using Talapker.Notifications.Firebase;
using Talapker.Notifications.Models;

namespace Talapker.Notifications.Features.Commands;

public class SendPushHandler
{
    private readonly IFirebaseSender _firebaseSender;

    public SendPushHandler(IFirebaseSender firebaseSender)
    {
        _firebaseSender = firebaseSender;
    }

    public async Task Handle(SendPushCommand command, IDocumentSession session)
    {
        var devices = await session.Query<UserDevice>()
            .Where(d => d.UserId == command.UserId && d.IsActive)
            .ToListAsync();

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = command.UserId,
            Type = command.Type ?? "General",
            Subject = command.Title,
            Body = command.Body,
            DeepLink = command.DeepLink,
            ImageUrl = command.ImageUrl,
            Data = command.Data,
            PushSent = false,
            CreatedAt = DateTime.UtcNow
        };

        session.Store(notification);
        await session.SaveChangesAsync();

        if (!devices.Any())
            return;

        try
        {
            var tokens = devices.Select(d => d.DeviceToken).ToList();

            await _firebaseSender.SendToMultipleTokensAsync(
                tokens,
                command.Title,
                command.Body,
                command.DeepLink,
                command.Data
            );

            notification.PushSent = true;
            notification.PushSentAt = DateTime.UtcNow;
            session.Update(notification);
            await session.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new DomainException($"Notifications stored but push delivery failed: {ex.Message}", 500);
        }
    }
}