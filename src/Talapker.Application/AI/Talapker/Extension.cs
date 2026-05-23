using Microsoft.Extensions.DependencyInjection;
using Qdrant.Client;
using Talapker.Application.AI.Knowledge;

namespace Talapker.Application.AI.Talapker;

public static class Extension
{
    public static IServiceCollection AddTalapkerAgent(this IServiceCollection services)
    {
        services.AddSingleton(_ => new QdrantClient("localhost", 6334));
        services.AddScoped<IKnowledgeSearchService, KnowledgeSearchService>();
        services.AddScoped<ITalapkerAgent, TalapkerAgent>();
        return services;
    }
}