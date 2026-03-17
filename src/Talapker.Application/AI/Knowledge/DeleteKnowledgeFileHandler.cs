using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using Talapker.Infrastructure.Data;

namespace Talapker.Application.AI.Knowledge;

public class DeleteKnowledgeFileHandler
{
    private const string QdrantCollection = "knowledge";

    public async Task Handle
    (
        DeleteKnowledgeFile message,
        TalapkerDbContext db,
        IAmazonS3 s3Client,
        IConfiguration configuration
    )
    {
        var file = await db.KnowledgeFiles
            .Include(f => f.Entries)
            .FirstOrDefaultAsync(f => f.Id == message.FileId && f.InstitutionId == message.InstitutionId);

        if (file is null)
            throw new InvalidOperationException("Файл не найден");

        var qdrant = new QdrantClient(
            configuration["Qdrant:Host"] ?? "localhost",
            int.Parse(configuration["Qdrant:Port"] ?? "6334")
        );

        var entryIds = file.Entries.Select(e => new PointId { Uuid = e.Id.ToString() }).ToList();
        if (entryIds.Count > 0)
            await qdrant.DeleteAsync(QdrantCollection, entryIds);

        await s3Client.DeleteObjectAsync(new DeleteObjectRequest
        {
            BucketName = configuration["AWS3Settings:Bucket"]!,
            Key = file.StorageKey
        });

        db.KnowledgeFiles.Remove(file);
        await db.SaveChangesAsync();
    }
}