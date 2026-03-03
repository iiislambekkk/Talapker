using Marten;
using Microsoft.AspNetCore.Identity.UI.Services;
using Talapker.Application;
using Talapker.Infrastructure.Exceptions;
using Talapker.Notifications.Models;

namespace Talapker.Notifications.Features.Commands;

public record SendEmailCommand(
    string UserId,
    string Email,
    string Subject,
    string Body,
    string? HtmlBody = null,
    string? Type = "General",
    Dictionary<string, string>? Data = null
);

public class SendEmailHandler
{
    
    public async Task Handle(SendEmailCommand command, IDocumentSession session, IEmailSender emailSender)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = Guid.Parse(command.UserId),
            Email = command.Email,
            Type = command.Type ?? "General",
            Subject = command.Subject,
            Body = command.Body,
            HtmlBody = command.HtmlBody,
            Data = command.Data,
            EmailSent = false,
            CreatedAt = DateTime.UtcNow
        };
        
        session.Store(notification);
        await session.SaveChangesAsync();
        
        try
        {
            await emailSender.SendEmailAsync(command.Email, command.Subject, 
                command.HtmlBody ?? command.Body);
            
            notification.EmailSent = true;
            notification.EmailSentAt = DateTime.UtcNow;
            session.Update(notification);
            await session.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new DomainException("Notification stored but email delivery failed", 500);
        }
    }
}