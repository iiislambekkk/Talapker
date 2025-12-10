using Microsoft.Extensions.DependencyInjection;

namespace Talapker.Infrastructure.Data.Seeding;

public static class DependencyInjection
{
    public static IServiceCollection AddDataSeeding(this IServiceCollection services)
    {
        services.AddScoped<SeedData>();

        return services;
    }
}