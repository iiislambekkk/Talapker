using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Resend;
using Talapker.Infrastructure.Email;

namespace Talapker.Infrastructure.Vault;

public static class Extension
{
    public static IServiceCollection AddVaultStore(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<IVaultStore, VaultStore>();
        
        return services;
    }
}