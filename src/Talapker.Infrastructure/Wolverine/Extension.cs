using System.Reflection;
using JasperFx;
using JasperFx.Core;
using JasperFx.Resources;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Npgsql;
using Wolverine;
using Wolverine.ErrorHandling;
using Wolverine.FluentValidation;
using Wolverine.Postgresql;

namespace Talapker.Infrastructure.Wolverine;

public static class Extensions
{
    public static IHostBuilder AddAndConfigureWolverine
    (
        this IHostBuilder host,
        IConfiguration configuration,
        Assembly assembly
    )
    {
        
        host.UseWolverine(opts =>
        {
            opts.Policies.OnException<ConcurrencyException>().RetryTimes(3);
            
            opts.Policies.OnException<NpgsqlException>()
                .RetryWithCooldown(50.Milliseconds(), 100.Milliseconds(), 250.Milliseconds());
            
            opts.Discovery.IncludeAssembly(assembly);
            
            var connectionString = configuration.GetConnectionString("PostgresSQL");

            if (connectionString == null)
            {
                throw new NullReferenceException();
            }

            // opts.UseKafka("localhost:9094").AutoProvision();
            
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