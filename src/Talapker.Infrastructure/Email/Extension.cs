using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Resend;

namespace Talapker.Infrastructure.Email;

public static class Extension
{
    public static IServiceCollection AddEmailSender(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient<ResendClient>();
        services.Configure<ResendClientOptions>( o =>
        {
            o.ApiToken = configuration["Resend:ApiToken"]!;
        } );
        services.AddTransient<IResend, ResendClient>();
        
        services.AddTransient<IEmailSender, ResendEmailSender>();
        
        return services;
    }
}