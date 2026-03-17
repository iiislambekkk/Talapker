using Elastic.Ingest.Elasticsearch.DataStreams;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using Elastic.Serilog.Sinks;

namespace Talapker.Web.Logging;

public static class Extensions
{
    public static IServiceCollection AddAndConfigureSerilog
    (
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .MinimumLevel.Override("Marten", LogEventLevel.Warning)
            .MinimumLevel.Override("Wolverine", LogEventLevel.Warning)
            .MinimumLevel.Override("Npgsql", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
                theme: AnsiConsoleTheme.Sixteen
            )
            .WriteTo.Logger(lc => lc
                .Filter.ByExcluding(logEvent =>
                    logEvent.Properties.TryGetValue("SourceContext", out var source) &&
                    source.ToString().Contains("SecurityStampMiddleware")
                )
                .Filter.ByExcluding(logEvent =>
                    logEvent.Properties.TryGetValue("SourceContext", out var source) &&
                    source.ToString().Contains("EntityFrameworkCore")
                )
                .Filter.ByExcluding(logEvent =>
                    logEvent.Properties.TryGetValue("RequestPath", out var path) &&
                    (
                        path.ToString().Contains("health") ||
                        path.ToString().Contains("favicon")
                    )
                )
                .WriteTo.Elasticsearch(
                    new[] { new Uri("http://localhost:9200") },
                    opts =>
                    {
                        opts.DataStream = new DataStreamName("logs", "talapker", "default");
                    }
                )
            )
            .CreateLogger();

        services.AddSerilog();

        return services;
    }
}