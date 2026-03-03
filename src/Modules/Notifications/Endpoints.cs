using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Talapker.Application;
using Talapker.Notifications.Features.Commands;
using Talapker.Notifications.Features.Queries;
using Talapker.Notifications.Models;
using Wolverine;

namespace Talapker.Notifications;

public static class NotificationsEndpoints
{
    public static IEndpointRouteBuilder MapNotificationsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/notifications").WithTags("Notifications");
        
        // Device management - POST with body
        group.MapPost("/devices", async ([FromBody] RegisterDeviceCommand command, IMessageBus bus) =>
        {
            await bus.InvokeAsync(command);
            return Results.Ok();
        }).WithName("RegisterDevice");
        
        // Device management - DELETE with route parameters (not body)
        group.MapDelete("/devices/{deviceToken}", async (string deviceToken, [FromBody] UnregisterDeviceCommand? command, IMessageBus bus) =>
        {
            // Option 1: Create command from route parameter
            var unregisterCommand = new UnregisterDeviceCommand(deviceToken);
            await bus.InvokeAsync(unregisterCommand);
            
            // Option 2: Or use command from body if you need more fields
            // if (command != null)
            // {
            //     await bus.InvokeAsync(command);
            // }
            
            return Results.Ok();
        }).WithName("UnregisterDevice");
        
        // Sending - POST with body
        group.MapPost("/email", async ([FromBody] SendEmailCommand command, IMessageBus bus) =>
        {
            var notification = await bus.InvokeAsync<Notification>(command);
            return Results.Ok(notification);
        }).WithName("SendEmail");
        
        group.MapPost("/push", async ([FromBody] SendPushCommand command, IMessageBus bus) =>
        {
            var notifications = await bus.InvokeAsync<List<Notification>>(command);
            return Results.Ok(notifications);
        }).WithName("SendPush");
        
        // Reading status - POST with route + query parameters
        group.MapPost("/{notificationId:guid}/read", async (Guid notificationId, [FromQuery] Guid userId, IMessageBus bus) =>
        {
            var command = new MarkAsReadCommand(notificationId, userId);
            await bus.InvokeAsync(command);
            return Results.Ok();
        }).WithName("MarkAsRead");
        
        group.MapPost("/users/{userId:guid}/read-all", async (Guid userId, IMessageBus bus) =>
        {
            var command = new MarkAllAsReadCommand(userId);
            await bus.InvokeAsync(command);
            return Results.Ok();
        }).WithName("MarkAllAsRead");
        
        // Retrieval - GET with route + query parameters
        group.MapGet("/users/{userId:guid}", async (Guid userId, int? limit, bool? onlyUnread, IMessageBus bus) =>
        {
            var query = new GetUserNotificationsQuery(userId, limit, onlyUnread);
            var notifications = await bus.InvokeAsync<List<Notification>>(query);
            return Results.Ok(notifications);
        }).WithName("GetUserNotifications");
        
        group.MapGet("/{notificationId:guid}/users/{userId:guid}", async (Guid notificationId, Guid userId, IMessageBus bus) =>
        {
            var query = new GetNotificationQuery(notificationId, userId);
            var notification = await bus.InvokeAsync<Notification?>(query);
            
            if (notification == null)
                return Results.NotFound();
                
            return Results.Ok(notification);
        }).WithName("GetNotification");
        
        return app;
    }
}