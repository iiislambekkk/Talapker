using Talapker.Infrastructure.S3;

namespace Talapker.Application.File.DeleteFile;

public class DeleteFileHandler
{
    public async Task Handle(DeleteFileCommand command, IS3StorageService s3Service)
    {
        await s3Service.DeleteAsync(command.Key);
    }
}