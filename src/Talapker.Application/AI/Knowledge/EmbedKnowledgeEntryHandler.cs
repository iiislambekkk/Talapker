using Microsoft.Extensions.Configuration;
using OpenAI;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using Talapker.Infrastructure.Data;
using Talapker.Infrastructure.Data.Assistant;

namespace Talapker.Application.AI.Knowledge;

public class EmbedKnowledgeEntryHandler
{
    private const string QdrantCollection = "knowledge";
    private const int VectorSize = 1536;

    public async Task Handle(EmbedKnowledgeEntry message, TalapkerDbContext db, IConfiguration configuration)
    {
        var entry = new KnowledgeEntry
        {
            InstitutionId = message.InstitutionId,
            SourceFileId = message.SourceFileId,
            Question = message.Question,
            Answer = message.Answer,
            Tags = message.Tags.ToArray()
        };

        db.KnowledgeEntries.Add(entry);
        await db.SaveChangesAsync();

        await EmbedAndUpsertAsync(entry, message.SourceFileId.ToString(), "", "", configuration);
    }

    internal async Task EmbedAndUpsertAsync(
        KnowledgeEntry entry,
        string sourceFileId,
        string sourceFileName,
        string sourceFileKey,
        IConfiguration configuration
    )
    {
        var openAIClient = new OpenAIClient(configuration["OpenAIKey"]!);
        var embeddingClient = openAIClient.GetEmbeddingClient("text-embedding-3-small");
        var embeddingResult = await embeddingClient.GenerateEmbeddingAsync(entry.Question);
        var embedding = embeddingResult.Value.ToFloats().ToArray();

        var qdrant = new QdrantClient(
            configuration["Qdrant:Host"] ?? "localhost",
            int.Parse(configuration["Qdrant:Port"] ?? "6334")
        );
        await EnsureCollectionAsync(qdrant);

        await qdrant.UpsertAsync(QdrantCollection,
        [
            new PointStruct
            {
                Id = new PointId { Uuid = entry.Id.ToString() },
                Vectors = embedding,
                Payload =
                {
                    ["question"] = entry.Question,
                    ["answer"] = entry.Answer,
                    ["tags"] = string.Join(",", entry.Tags),
                    ["institution_id"] = entry.InstitutionId.ToString(),
                    ["source_file_id"] = sourceFileId,
                    ["source_file_name"] = sourceFileName,
                    ["source_file_key"] = sourceFileKey
                }
            }
        ]);
    }

    private static async Task EnsureCollectionAsync(QdrantClient qdrant)
    {
        var collections = await qdrant.ListCollectionsAsync();
        if (collections.Any(c => c == QdrantCollection)) return;

        await qdrant.CreateCollectionAsync(QdrantCollection, new VectorParams
        {
            Size = VectorSize,
            Distance = Distance.Cosine
        });
    }
}