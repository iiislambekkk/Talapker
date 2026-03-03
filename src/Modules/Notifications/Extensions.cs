using System.Text.Json;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using JasperFx;
using Marten;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Talapker.Notifications.Firebase;
using Talapker.Notifications.Models;

namespace Talapker.Notifications;

public static class Extensions
{
    public static IServiceCollection AddMartenDb(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.AddMarten(opts =>
            {
                opts.Connection(configuration.GetConnectionString("Marten")!);
                opts.DatabaseSchemaName = "notifications";

                opts.RegisterDocumentType<UserDevice>();
                opts.RegisterDocumentType<Notification>();
                
                opts.AutoCreateSchemaObjects = AutoCreate.All;
            })
            .UseLightweightSessions();

        return serviceCollection;
    }

    public static IServiceCollection AddFirebase(this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        var firebaseConfig = configuration.GetSection("Firebase").Get<Dictionary<string, string>>();

        if (FirebaseApp.DefaultInstance == null)
        {
            FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromJson(JsonSerializer.Serialize(firebaseConfig))
            });
        }

        serviceCollection.AddScoped<IFirebaseSender, FirebaseSender>();

        return serviceCollection;
    }

    public static IServiceCollection AddNotificationModule(this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {

        serviceCollection.AddMartenDb(configuration);
        serviceCollection.AddFirebase(configuration);
        serviceCollection.ConfigureWolverineForNotifications();
        
        return serviceCollection;
    }
    
    public static IEndpointRouteBuilder MapNotificationModuleRoutes(this IEndpointRouteBuilder builder)
    {

        builder.MapNotificationsEndpoints();
        
        return builder;
    }
}