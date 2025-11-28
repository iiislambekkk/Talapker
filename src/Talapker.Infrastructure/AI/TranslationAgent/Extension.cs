using Microsoft.Extensions.DependencyInjection;

namespace Talapker.Infrastructure.AI.TranslationAgent;

public static class Extension
{
    public static IServiceCollection AddTranslationAgent(this IServiceCollection services)
    {
        services.AddScoped<ITranslationService, TranslationService>();
        return services;
    }
}