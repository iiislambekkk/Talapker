namespace Talapker.Web.AppExtensions;

public static class CorsExtension
{
    public static IServiceCollection ConfigureCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins(
                        "http://localhost:63342", 
                        "http://localhost:5026",  
                        "http://localhost:3001",
                        "https://frontend.mektep32.org"
                    )
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });
        
        return services;
    }
}