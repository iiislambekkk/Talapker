using System.Reflection;
using JasperFx;
using JasperFx.Core;
using JasperFx.Resources;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using Talapker.Infrastructure.Secrets;
using Wolverine;
using Wolverine.ErrorHandling;
using Wolverine.FluentValidation;
using Wolverine.Kafka;
using Wolverine.Postgresql;

namespace Talapker.Infrastructure.Wolverine;

public static class Extensions
{
    public static IHostBuilder AddAndConfigureWolverine
    (
        this IHostBuilder host,
        IConfiguration configuration,
        List<Assembly> assemblies
    )
    {
        
        host.UseWolverine(opts =>
        {
            opts.Policies.OnException<ConcurrencyException>().RetryTimes(3);
            
            opts.Policies.OnException<NpgsqlException>()
                .RetryWithCooldown(50.Milliseconds(), 100.Milliseconds(), 250.Milliseconds());

            foreach (var assembly in assemblies)
            {
                opts.Discovery.IncludeAssembly(assembly);
            }
            
            var secretProvider = new SecretProvider(configuration, NullLogger<SecretProvider>.Instance);
            var connectionString = secretProvider
                .GetRequiredAsync("ConnectionStrings:PostgresSQL")
                .GetAwaiter()
                .GetResult();
            
            opts.PersistMessagesWithPostgresql(connectionString, "wolverine");
    
            opts.Policies.UseDurableOutboxOnAllSendingEndpoints();
            
            opts.Policies.UseDurableLocalQueues();
            
            opts.Policies.UseDurableInboxOnAllListeners();
            
            opts.Policies.AutoApplyTransactions();
            
            opts.UseFluentValidation();

            host.UseResourceSetupOnStartup();

        });
        
        return host;
    }
}