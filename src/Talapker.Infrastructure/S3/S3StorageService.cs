using System.Net;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using AWS3Settings = Talapker.Infrastructure.Settings.AWS3Settings;
using UserAccess_Infrastructure_Settings_AWS3Settings = Talapker.Infrastructure.Settings.AWS3Settings;


namespace Talapker.Infrastructure.S3;

public class S3StorageService(IAmazonS3 s3Client, IConfiguration configuration) : IS3StorageService
{
    private readonly string _bucket = configuration.GetSection(nameof(AWS3Settings)).Get<UserAccess_Infrastructure_Settings_AWS3Settings>()!.Bucket;

    public async Task<string> GetPresignedUrl(string key, string contentType)
    {
        AWSConfigsS3.DisableDefaultChecksumValidation = true;
        
        var request = new GetPreSignedUrlRequest()
        {
            BucketName = _bucket,
            Key = key,
            Verb = HttpVerb.PUT,
            ContentType =  contentType,
            Expires = DateTime.Now.AddDays(7)
        };
        
        var preSignedUrl = await s3Client.GetPreSignedURLAsync(request);
        return preSignedUrl;
    }

    public async Task<bool> DeleteAsync(string key)
    {
        var request = new DeleteObjectRequest()
        {
            BucketName = _bucket,
            Key = key
        };

        var response = await s3Client.DeleteObjectAsync(request);

        return response.HttpStatusCode == HttpStatusCode.NoContent;
    }
}