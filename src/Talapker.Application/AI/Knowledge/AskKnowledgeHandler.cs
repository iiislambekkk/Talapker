using System.Text;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using Talapker.Infrastructure;
using Talapker.Infrastructure.Secrets;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;
using Filter = Qdrant.Client.Grpc.Filter;

namespace Talapker.Application.AI.Knowledge;

public class AskKnowledgeHandler(IKnowledgeSearchService knowledgeSearch, IConfiguration configuration)
{
    public async Task<AskKnowledgeResult> Handle(AskKnowledge message)
    {
        var searchResult = await knowledgeSearch.SearchAsync(new KnowledgeSearchOptions(
            Question: message.Question,
            InstitutionId: message.InstitutionId
        ));

        var apiKey = configuration[SecretsKeys.OpenAIKey];
        var openAIClient = new OpenAIClient(apiKey);

        var contextBuilder = new StringBuilder();
        contextBuilder.AppendLine("Контекст из базы знаний:");

        var sources = new List<KnowledgeSourceRef>();

        foreach (var chunk in searchResult.Chunks)
        {
            contextBuilder.AppendLine($"- Вопрос: {chunk.Question}");
            contextBuilder.AppendLine($"  Ответ: {chunk.Answer}");
            contextBuilder.AppendLine($"  Источник ID: {chunk.SourceFileId}");
            contextBuilder.AppendLine();

            if (!string.IsNullOrEmpty(chunk.SourceFileId) && sources.All(s => s.FileKey != chunk.SourceFileId))
            {
                sources.Add(new KnowledgeSourceRef(
                    chunk.SourceFileId,
                    chunk.SourceFileId,
                    chunk.Question
                ));
            }
        }

        var agent = openAIClient
            .GetChatClient("gpt-5-nano-2025-08-07")
            .AsIChatClient()
            .CreateAIAgent(new ChatClientAgentOptions
            {
                ChatOptions = new ChatOptions
                {
                    Instructions = $"""
                        Ты помощник по вопросам поступления в университет.
                        Отвечай только на основе предоставленного контекста.
                        Если ответа нет в контексте — скажи что не знаешь.
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
}