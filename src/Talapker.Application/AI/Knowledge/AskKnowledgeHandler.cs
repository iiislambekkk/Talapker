using System.Text;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;
using Filter = Qdrant.Client.Grpc.Filter;

namespace Talapker.Application.AI.Knowledge;

public class AskKnowledgeHandler(IConfiguration configuration)
{
    private const string QdrantCollection = "knowledge";

    public async Task<AskKnowledgeResult> Handle(AskKnowledge message)
    {
        var openAIClient = new OpenAIClient(configuration["OpenAIKey"]!);

        var embeddingClient = openAIClient.GetEmbeddingClient("text-embedding-3-small");
        var embeddingResult = await embeddingClient.GenerateEmbeddingAsync(message.Question);
        var queryEmbedding = embeddingResult.Value.ToFloats().ToArray();

        var qdrant = CreateQdrantClient();
        var searchResults = await qdrant.SearchAsync(
            QdrantCollection,
            queryEmbedding,
            filter: new Filter
            {
                Must =
                {
                    new Condition
                    {
                        Field = new FieldCondition
                        {
                            Key = "institution_id",
                            Match = new Match { Text = message.InstitutionId.ToString() }
                        }
                    }
                }
            },
            limit: 3
        );

        var contextBuilder = new StringBuilder();
        contextBuilder.AppendLine("Контекст из базы знаний:");

        var sources = new List<KnowledgeSourceRef>();

        foreach (var r in searchResults)
        {
            contextBuilder.AppendLine($"- Вопрос: {r.Payload["question"].StringValue}");
            contextBuilder.AppendLine($"  Ответ: {r.Payload["answer"].StringValue}");
            contextBuilder.AppendLine($"  Источник: {r.Payload["source_file_name"].StringValue}");
            contextBuilder.AppendLine();

            var fileKey = r.Payload["source_file_key"].StringValue;
            if (!string.IsNullOrEmpty(fileKey) && sources.All(s => s.FileKey != fileKey))
            {
                sources.Add(new KnowledgeSourceRef(
                    r.Payload["source_file_name"].StringValue,
                    fileKey,
                    r.Payload["question"].StringValue
                ));
            }
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
                Contents = [new TextContent(message.Question)]
            }
        ]);

        return new AskKnowledgeResult(
            Answer: response.Text ?? "Не удалось получить ответ",
            Sources: sources
        );
    }

    private QdrantClient CreateQdrantClient() =>
        new(
            configuration["Qdrant:Host"] ?? "localhost",
            int.Parse(configuration["Qdrant:Port"] ?? "6334")
        );
}