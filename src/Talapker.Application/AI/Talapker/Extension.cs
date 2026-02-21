using Microsoft.Extensions.DependencyInjection;
using Talapker.Application.AI.TranslationAgent;

namespace Talapker.Application.AI.Talapker;

public static class Extension
{
    public static IServiceCollection AddTalapkerAgent(this IServiceCollection services)
    {
        services.AddScoped<ITalapkerAgent, TalapkerAgent>();
        return services;
    }
}