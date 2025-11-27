using Amazon.Runtime;
using Amazon.S3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Talapker.Infrastructure.Settings;

namespace Talapker.Infrastructure.S3;

public static class Extension
{
    public static IServiceCollection AddS3Storage(this IServiceCollection services,  IConfiguration configuration)
    {
        var awsSettings = configuration.GetSection(nameof(AWS3Settings)).Get<AWS3Settings>()!;
        
        services.AddSingleton<IAmazonS3>(conf =>
            {
                var s3Config = new AmazonS3Config()
                {
                    ServiceURL = awsSettings.Endpoint
                };
                
                var credentials = new BasicAWSCredentials(awsSettings.AccessKey, awsSettings.SecretKey);

                return new AmazonS3Client(credentials,  s3Config);
            }
        );

        services.AddScoped<IS3StorageService, S3StorageService>();

        return services;
    }
}