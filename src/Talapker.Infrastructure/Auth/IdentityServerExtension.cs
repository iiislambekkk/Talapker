using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Talapker.Infrastructure.Auth.HostedServices;
using Talapker.Infrastructure.Data;
using Talapker.Infrastructure.Settings;

namespace Talapker.Infrastructure.Auth;


public static class IdentityServerExtension
{
    
    public static IServiceCollection AddIdentityServer(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection(nameof(OAuthSettings)).Get<OAuthSettings>();

        if (settings == null)
        {
            throw new NullReferenceException("OAuth settings not found. Please be sure that you provide \"OAuth\" settings in the configuration.");
        }
        
        services.AddOpenIddict()
            .AddCore(opt =>
            {
                opt.UseEntityFrameworkCore()
                    .UseDbContext<TalapkerDbContext>();
            })
            .AddServer(opt =>
            {
                opt.SetIssuer(settings.Issuer);

                opt.SetRefreshTokenLifetime(TimeSpan.FromDays(7));
                
                // TODO: Change with real certificates
                opt
                    .AddEphemeralEncryptionKey()
                    .AddEphemeralSigningKey()
                    .DisableAccessTokenEncryption();
                
                opt
                    .SetAuthorizationEndpointUris("/connect/authorize")
                    .SetTokenEndpointUris("/connect/token");
                
                opt
                    .SetEndSessionEndpointUris("/connect/endsession")
                    .SetUserInfoEndpointUris("/connect/userinfo");
                
                opt
                    .AllowClientCredentialsFlow()
                    .AllowRefreshTokenFlow()
                    .AllowAuthorizationCodeFlow()
                    .RequireProofKeyForCodeExchange();

                opt.AddDevelopmentEncryptionCertificate()
                    .AddDevelopmentSigningCertificate();
                
                opt.UseAspNetCore()
                    .EnableTokenEndpointPassthrough()
                    .EnableAuthorizationEndpointPassthrough()
                    .EnableEndSessionEndpointPassthrough()
                    .EnableUserInfoEndpointPassthrough()
                    // TODO: In production, this option must be removed, and all the requests must be made through HTTPS. Just add nginx headers forwarder.
                    .DisableTransportSecurityRequirement();
            })
            .AddValidation(opt =>
                {
                    opt.UseLocalServer();
                    opt.UseAspNetCore();
                    opt.AddAudiences("IdentityServer");
                }
            );
        
        services.Configure<OAuthSettings>(configuration.GetSection(nameof(OAuthSettings)));
        
        if (configuration.GetValue<bool>("seed"))
        {
            services.AddHostedService<IdentityServerSeedHostedService>();
        }
        
        return services;
    }
    
}