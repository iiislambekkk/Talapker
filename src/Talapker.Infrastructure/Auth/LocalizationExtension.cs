using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;

namespace Talapker.Infrastructure.Auth;

public static class LocalizationExtension
{
    public static IServiceCollection AddLocalizedRazor(this IServiceCollection service)
    {
        service.AddLocalization(options => options.ResourcesPath = "Resources");
        service
            .AddRazorPages()
            .AddDataAnnotationsLocalization(options => {
                options.DataAnnotationLocalizerProvider = (type, factory) =>
                    factory.Create(typeof(SharedResource));
            })
            .AddViewLocalization(); 
    
        var supportedCultures = new[] { "en", "ru", "kk" };

        service.Configure<RequestLocalizationOptions>(options =>
        {
            var cultures = supportedCultures.Select(c => new CultureInfo(c)).ToList();
            options.DefaultRequestCulture = new RequestCulture("kk");
            options.SupportedCultures = cultures;
            options.SupportedUICultures = cultures;
            options.RequestCultureProviders.Insert(0, new CookieRequestCultureProvider());
            options.RequestCultureProviders.Insert(1, new AcceptLanguageHeaderRequestCultureProvider());
        });
        
        return service;
    }
}