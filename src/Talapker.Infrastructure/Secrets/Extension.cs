using Microsoft.Extensions.DependencyInjection;

namespace Talapker.Infrastructure.Secrets;

public static class Extension
{
    public static IServiceCollection AddSecretsProvider(this IServiceCollection services)
    {
        services.AddSingleton<ISecretProvider, SecretProvider>();
        return services;
    }
}