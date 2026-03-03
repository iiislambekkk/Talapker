using Microsoft.Extensions.DependencyInjection;
using Talapker.Notifications.Features.Commands;
using Wolverine;
using Wolverine.Kafka;

namespace Talapker.Notifications;

public static class WolverineConfig
{
    public static void ConfigureWolverineForNotifications(this IServiceCollection services)
    {
        services.AddSingleton<IWolverineExtension, NotificationsWolverineExtension>();
    }
}

public class NotificationsWolverineExtension : IWolverineExtension
{
    public void Configure(WolverineOptions options)
    {
        options.UseKafka("localhost:9094").AutoProvision();
        
        options.PublishMessage<SendPushCommand>()
            .ToKafkaTopic("notifications-push");
        
        options.PublishMessage<SendEmailCommand>()
            .ToKafkaTopic("notifications-email");
        
        options.ListenToKafkaTopic("notifications-push");
        options.ListenToKafkaTopic("notifications-email");
    }
}