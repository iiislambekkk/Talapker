using System.Text.Json;
using JasperFx;
using Marten;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Talapker.TelegramBot.Models;

namespace Talapker.TelegramBot;

public static class Extensions
{
    public static IServiceCollection AddMartenDb(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.ConfigureMarten(opts =>
        {

            opts.Schema.For<Models.TelegramBot>()
                .Index(x => x.InstitutionId)
                .Index(x => x.BotToken)
                .DatabaseSchemaName("telegram");

            opts.Schema.For<TelegramBotSession>()
                .Index(x => x.BotId)
                .Index(x => x.TelegramUserId)
                .DatabaseSchemaName("telegram");


            opts.AutoCreateSchemaObjects = AutoCreate.All;
        });

        return serviceCollection;
    }


    public static IServiceCollection AddTelegramBotModule(this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {

        serviceCollection.AddMartenDb(configuration);
        
        serviceCollection.AddSingleton<TelegramClientFactory>();
        
        return serviceCollection;
    }
}