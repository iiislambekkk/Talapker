
namespace Talapker.Infrastructure.S3;

public interface IS3StorageService
{
    Task<string> GetPresignedUrl(string key, string contentType);
    Task<bool> DeleteAsync(string key);
}