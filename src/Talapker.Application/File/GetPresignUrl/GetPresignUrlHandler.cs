using Microsoft.AspNetCore.Identity;
using Talapker.Infrastructure.Data;
using Talapker.Infrastructure.Exceptions;
using Talapker.Infrastructure.S3;
using Wolverine.Runtime;

namespace Talapker.Application.File.GetPresignUrl;

public class GetPresignUrlHandler
{
    public async Task<GetPresignUrlResponse> Handle
    (
        GetPresignUrlCommand command,
        IS3StorageService s3Service,
        MessageBus bus, 
        CancellationToken ct
    )
    {
        var uniqueKey = $"{Guid.NewGuid()}-{command.FileName}";
        var presignedUrl = await s3Service.GetPresignedUrl(uniqueKey, command.ContentType);

        return new(presignedUrl, uniqueKey);
    }
}
