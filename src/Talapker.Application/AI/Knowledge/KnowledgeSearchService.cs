using Microsoft.Extensions.Configuration;
using OpenAI;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using Talapker.Infrastructure;
using Talapker.Infrastructure.Secrets;
using Filter = Qdrant.Client.Grpc.Filter;

namespace Talapker.Application.AI.Knowledge;

public record KnowledgeSearchOptions(
    string Question,
    Guid InstitutionId,
    int TopK = 10,
    float ScoreThreshold = 0.5f
);

public record KnowledgeChunk(
    string Question,
    string Answer,
    string SourceFileId,
    float Score
);

public record KnowledgeSearchResult(IReadOnlyList<KnowledgeChunk> Chunks);

public interface IKnowledgeSearchService
{
    Task<KnowledgeSearchResult> SearchAsync(KnowledgeSearchOptions options, CancellationToken ct = default);
}

public sealed class KnowledgeSearchService(
    IConfiguration configuration,
    QdrantClient qdrant
) : IKnowledgeSearchService
{
    private const string QdrantCollection = "knowledge";

    private static readonly WithPayloadSelector PayloadSelector = new()
    {
        Include = new PayloadIncludeSelector
        {
            Fields = { "question", "answer", "source_file_id" }
        }
    };

    public async Task<KnowledgeSearchResult> SearchAsync(KnowledgeSearchOptions options, CancellationToken ct = default)
    {
        var apiKey = configuration[SecretsKeys.OpenAIKey];
        var openAIClient = new OpenAIClient(apiKey);

        var embeddingClient = openAIClient.GetEmbeddingClient("text-embedding-3-small");
        var embeddingResult = await embeddingClient.GenerateEmbeddingAsync(options.Question, cancellationToken: ct);
        var queryEmbedding = embeddingResult.Value.ToFloats().ToArray();

        var filter = new Filter
        {
            Must =
            {
                new Condition
                {
                    Field = new FieldCondition
                    {
                        Key = "institution_id",
                        Match = new Match { Text = options.InstitutionId.ToString() }
                    }
                }
            }
        };

        var results = await qdrant.SearchAsync(
            QdrantCollection,
            queryEmbedding,
            filter: filter,
            scoreThreshold: options.ScoreThreshold,
            limit: (ulong)options.TopK,
            payloadSelector: PayloadSelector
        );

        if (results.Count == 0)
            return new KnowledgeSearchResult([]);

        var chunks = results
            .Select(r => new KnowledgeChunk(
                Question: r.Payload["question"].StringValue,
                Answer: r.Payload["answer"].StringValue,
                SourceFileId: r.Payload["source_file_id"].StringValue,
                Score: r.Score
            ))
            .ToList();

        return new KnowledgeSearchResult(chunks);
    }
}