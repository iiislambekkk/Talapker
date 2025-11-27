using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Talapker.Infrastructure.Data.UserAccess;

public static class Extension
{
    public static IServiceCollection AddTalapkerDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgresSQL");

        if (connectionString == null)
        {
            throw new NullReferenceException();
        }
        
        services.AddDbContext<TalapkerDbContext>
        (
            options =>
            {
                options.UseNpgsql(connectionString);
                options.UseOpenIddict();
            },
            optionsLifetime: ServiceLifetime.Singleton
        );
        
        return services;
    }
}