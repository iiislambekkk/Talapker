using Marten;
using Talapker.Application;
using Talapker.Infrastructure.Exceptions;
using Talapker.Notifications.Firebase;
using Talapker.Notifications.Models;

namespace Talapker.Notifications.Features.Commands;

public record SendPushCommand(
    Guid UserId,
    string Title,
    string Body,
    string? DeviceToken = null,
    string? DeepLink = null,
    string? ImageUrl = null,
    string? Type = "General",
    Dictionary<string, string>? Data = null
);

public class SendPushHandler
{
    private readonly IFirebaseSender _firebaseSender;
    
    public SendPushHandler(IFirebaseSender firebaseSender)
    {
        _firebaseSender = firebaseSender;
    }
    
    public async Task Handle(SendPushCommand command, IDocumentSession session)
    {
        var notifications = new List<Notification>();
        
        var devices = await session.Query<UserDevice>()
            .Where(d => d.UserId == command.UserId && d.IsActive)
            .ToListAsync();
        
        if (!string.IsNullOrEmpty(command.DeviceToken))
        {
            devices = devices.Where(d => d.DeviceToken == command.DeviceToken).ToList();
        }
        
        if (!devices.Any())
        {
            return;
        }
        
        foreach (var device in devices)
        {
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
            notifications.Add(notification);
        }
        
        await session.SaveChangesAsync();
        
        try
        {
            var tokens = devices.Select(d => d.DeviceToken).ToList();
            
            // Using the simplified method with DeepLink
            await _firebaseSender.SendToMultipleTokensAsync(
                tokens, 
                command.Title, 
                command.Body, 
                command.DeepLink,
                command.Data
            );
            
            foreach (var notification in notifications)
            {
                notification.PushSent = true;
                notification.PushSentAt = DateTime.UtcNow;
                session.Update(notification);
            }
            
            await session.SaveChangesAsync();

            return;
        }
        catch (Exception ex)
        {
           throw new DomainException("Notifications stored but push delivery failed", 500);
        }
    }
}