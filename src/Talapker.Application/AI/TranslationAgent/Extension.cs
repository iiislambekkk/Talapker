using Microsoft.Extensions.DependencyInjection;

namespace Talapker.Application.AI.TranslationAgent;

public static class Extension
{
    public static IServiceCollection AddTranslationAgent(this IServiceCollection services)
    {
        services.AddScoped<ITranslationService, TranslationService>();
        return services;
    }
}