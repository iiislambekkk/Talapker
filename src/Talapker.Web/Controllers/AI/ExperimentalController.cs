using System.Text;
using System.Text.Json;
using Amazon.S3;
using Microsoft.Agents.AI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using OpenAI;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

namespace Talapker.Web.Controllers.AI;

public record AskRequest(string Question, IFormFile? File);
public record AskResponse(string Answer, List<SourceReference> Sources);
public record SourceReference(string FileName, string FileKey, string MatchedQuestion);
public record KnowledgeQnAPair(string Question, string Answer, List<string> Tags);
public record KnowledgeQnAResult(List<KnowledgeQnAPair> Pairs);

public record KnowledgeEntry
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Question { get; init; } = string.Empty;
    public string Answer { get; init; } = string.Empty;
    public List<string> Tags { get; init; } = [];
    public Guid SourceFileId { get; init; }
    public string SourceFileName { get; init; } = string.Empty;
    public string SourceFileKey { get; init; } = string.Empty;
}

[ApiController]
[Route("[controller]")]
public class ExperimentalController(IConfiguration configuration, IAmazonS3 s3Client) : ControllerBase
{
    private const string QdrantCollection = "knowledge";
    private const int VectorSize = 1536;

    [HttpPost("ask")]
    public async Task<string> AskAsync([FromForm] AskRequest request)
    {
        try
        {
            var openAIClient = new OpenAIClient(configuration["OpenAIKey"]!);
            var agent = openAIClient.GetChatClient("gpt-5-mini").AsIChatClient().CreateAIAgent();

            var contents = new List<AIContent> { new TextContent(request.Question) };

            if (request.File is { Length: > 0 })
            {
                using var ms = new MemoryStream();
                await request.File.CopyToAsync(ms);
                contents.Add(new DataContent(new ReadOnlyMemory<byte>(ms.ToArray()), GetMediaType(request.File.FileName)));
            }

            var response = await agent.RunAsync([new ChatMessage { Role = ChatRole.User, Contents = contents }]);
            return response.Text ?? "Не удалось получить ответ";
        }
        catch (Exception ex)
        {
            return $"Ошибка: {ex.Message}";
        }
    }

    [HttpPost("upload-knowledge")]
    public async Task<ActionResult> UploadKnowledgeAsync(IFormFile file)
    {
        if (file.Length == 0)
            return BadRequest("Файл пустой");

        var fileId = Guid.NewGuid();

        var openAIClient = new OpenAIClient(
            configuration["OpenAIKey"]!);

        var schema = AIJsonUtilities.CreateJsonSchema(typeof(KnowledgeQnAResult));
        var agent = openAIClient
            .GetChatClient("gpt-5-mini")
            .AsIChatClient()
            .CreateAIAgent(new ChatClientAgentOptions
            {
                ChatOptions = new ChatOptions
                {
                    ResponseFormat = ChatResponseFormat.ForJsonSchema(
                        schema: schema,
                        schemaName: "KnowledgeQnAResult",
                        schemaDescription: "List of question-answer pairs extracted from a university document"),
                    Instructions = """
                        You are a knowledge extraction assistant for a university admissions platform.
                        Extract meaningful question-answer pairs from the provided document.
                        Rules:
                        - Extract between 10 and 20 pairs depending on document size.
                        - Each question should be something a prospective student might ask.
                        - Each answer must be accurate and taken directly from the document.
                        - Assign multiple relevant tags per pair from: поступление, документы, стипендия, общежитие, экзамены, специальности, дедлайны, требования, контакты, оплата.
                        - Write all questions and answers in Russian.
                        - Do not invent information not present in the document.
                        """
                }
            });

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        var fileBytes = memoryStream.ToArray();

        var extractResponse = await agent.RunAsync(
        [
            new ChatMessage
            {
                Role = ChatRole.User,
                Contents =
                [
                    new TextContent("Extract knowledge base Q&A pairs from this document."),
                    new DataContent(new ReadOnlyMemory<byte>(fileBytes), GetMediaType(file.FileName))
                ]
            }
        ]);

        var result = extractResponse.Deserialize<KnowledgeQnAResult>(JsonSerializerOptions.Web);
        if (result is null)
            return StatusCode(500, "Не удалось обработать ответ модели");

        var fileKey = await UploadToR2Async(file, fileId, fileBytes);

        var qdrant = CreateQdrantClient();
        await EnsureCollectionAsync(qdrant);

        var embeddingClient = openAIClient.GetEmbeddingClient("text-embedding-3-small");

        foreach (var pair in result.Pairs)
        {
            var embeddingResult = await embeddingClient.GenerateEmbeddingAsync(pair.Question);
            var embedding = embeddingResult.Value.ToFloats().ToArray();

            var entry = new KnowledgeEntry
            {
                Question = pair.Question,
                Answer = pair.Answer,
                Tags = pair.Tags,
                SourceFileId = fileId,
                SourceFileName = file.FileName,
                SourceFileKey = fileKey
            };

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
                        ["source_file_id"] = entry.SourceFileId.ToString(),
                        ["source_file_name"] = entry.SourceFileName,
                        ["source_file_key"] = entry.SourceFileKey
                    }
                }
            ]);
        }

        return Ok(new
        {
            FileId = fileId,
            FileName = file.FileName,
            FileKey = fileKey,
            PairsExtracted = result.Pairs.Count
        });
    }

    [HttpPost("ask-knowledge")]
    public async Task<ActionResult<AskResponse>> AskKnowledgeAsync([FromBody] AskRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
            return BadRequest("Вопрос пустой");

        var openAIClient = new OpenAIClient(configuration["OpenAIKey"]!);

        var embeddingClient = openAIClient.GetEmbeddingClient("text-embedding-3-small");
        var embeddingResult = await embeddingClient.GenerateEmbeddingAsync(request.Question);
        var queryEmbedding = embeddingResult.Value.ToFloats().ToArray();

        var qdrant = CreateQdrantClient();
        var searchResults = await qdrant.SearchAsync(QdrantCollection, queryEmbedding, limit: 3);

        var relevantEntries = searchResults.Select(r => new KnowledgeEntry
        {
            Id = Guid.Parse(r.Id.Uuid),
            Question = r.Payload["question"].StringValue,
            Answer = r.Payload["answer"].StringValue,
            Tags = r.Payload["tags"].StringValue.Split(",").ToList(),
            SourceFileId = Guid.Parse(r.Payload["source_file_id"].StringValue),
            SourceFileName = r.Payload["source_file_name"].StringValue,
            SourceFileKey = r.Payload["source_file_key"].StringValue
        }).ToList();

        var contextBuilder = new StringBuilder();
        contextBuilder.AppendLine("Контекст из базы знаний:");
        foreach (var entry in relevantEntries)
        {
            contextBuilder.AppendLine($"- Вопрос: {entry.Question}");
            contextBuilder.AppendLine($"  Ответ: {entry.Answer}");
            contextBuilder.AppendLine($"  Источник: {entry.SourceFileName} (key: {entry.SourceFileKey})");
            contextBuilder.AppendLine();
        }

        var agent = openAIClient
            .GetChatClient("gpt-5-mini")
            .AsIChatClient()
            .CreateAIAgent(new ChatClientAgentOptions
            {
                ChatOptions = new ChatOptions
                {
                    Instructions = $"""
                        Ты помощник по вопросам поступления в университет.
                        Отвечай только на основе предоставленного контекста.
                        Если ответа нет в контексте — скажи что не знаешь.
                        В конце ответа укажи источники в формате: **Источник:** название файла.
                        Отвечай на русском языке.

                        {contextBuilder}
                        """
                }
            });

        var response = await agent.RunAsync(
        [
            new ChatMessage
            {
                Role = ChatRole.User,
                Contents = [new TextContent(request.Question)]
            }
        ]);

        var sources = relevantEntries
            .DistinctBy(e => e.SourceFileId)
            .Select(e => new SourceReference(e.SourceFileName, e.SourceFileKey, e.Question))
            .ToList();

        return Ok(new AskResponse(
            Answer: response.Text ?? "Не удалось получить ответ",
            Sources: sources
        ));
    }

    private QdrantClient CreateQdrantClient() =>
        new(
            configuration["Qdrant:Host"] ?? "localhost",
            int.Parse(configuration["Qdrant:Port"] ?? "6334")
        );

    private static async Task EnsureCollectionAsync(QdrantClient qdrant)
    {
        var collections = await qdrant.ListCollectionsAsync();
        if (collections.Any(c => c == QdrantCollection))
            return;

        await qdrant.CreateCollectionAsync(QdrantCollection, new VectorParams
        {
            Size = VectorSize,
            Distance = Distance.Cosine
        });
    }

    private async Task<string> UploadToR2Async(IFormFile file, Guid fileId, byte[] fileBytes)
    {
        var bucket = configuration["AWS3Settings:Bucket"]!;
        var key = $"knowledge/{fileId}/{file.FileName}";

        using var stream = new MemoryStream(fileBytes);
        await s3Client.PutObjectAsync(new Amazon.S3.Model.PutObjectRequest
        {
            InputStream = stream,
            Key = key,
            BucketName = bucket,
            ContentType = file.ContentType,
            UseChunkEncoding = false
        });

        return key;
    }

    private static string GetMediaType(string fileName) =>
        Path.GetExtension(fileName).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".pdf" => "application/pdf",
            ".txt" => "text/plain",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            _ => "application/octet-stream"
        };
}