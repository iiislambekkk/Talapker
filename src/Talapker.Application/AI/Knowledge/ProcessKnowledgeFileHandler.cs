using System.ClientModel;
using System.Text.Json;
using Amazon.Runtime.Internal.Util;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenAI;
using Talapker.Infrastructure.Data;
using Talapker.Infrastructure.Data.Assistant;
using Wolverine;

namespace Talapker.Application.AI.Knowledge;

public record KnowledgeQnAPair(string Question, string Answer, List<string> Tags);
public record KnowledgeQnAResult(List<KnowledgeQnAPair> Pairs);

public class ProcessKnowledgeFileHandler
{
    public async Task Handle(ProcessKnowledgeFile message, TalapkerDbContext db,
        IAmazonS3 s3Client,
        IConfiguration configuration,
        IMessageBus bus,
        ILogger<ProcessKnowledgeFileHandler> logger
    )
    {
        var file = await db.KnowledgeFiles.FindAsync(message.FileId);
        if (file is null) return;

        file.Status = KnowledgeFileStatus.Processing;
        await db.SaveChangesAsync();

        try
        {
            var bucket = configuration["AWS3Settings:Bucket"]!;
            var s3Response = await s3Client.GetObjectAsync(new GetObjectRequest
            {
                BucketName = bucket,
                Key = file.StorageKey
            });

            using var memoryStream = new MemoryStream();
            await s3Response.ResponseStream.CopyToAsync(memoryStream);
            var fileBytes = memoryStream.ToArray();

            logger.LogInformation("Downloaded file: {FileName}, size: {Size} bytes, mediaType: {MediaType}",
                file.FileName, fileBytes.Length, file.ContentType);

            var openAIClient = new OpenAIClient(new ApiKeyCredential(configuration["OpenAIKey"]!),
                new OpenAIClientOptions
                {
                    NetworkTimeout = TimeSpan.FromMinutes(10)
                });

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
                                       Extract question-answer pairs that a prospective student would actually ask.

                                       Rules:
                                       - Questions must be practical and student-facing: "Какие документы нужны для поступления?", not "Где находится таблица в документе?"
                                       - Never reference the document structure (pages, appendices, tables, paragraphs, sections).
                                       - Never mention "в документе", "в приложении", "на стр.", "п.", "см. выше/ниже".
                                       - Answers must be self-contained facts a student can act on directly.
                                       - If a table or appendix contains data (score conversions, deadlines, requirements) — embed the actual data into the answer, don't reference where it is.
                                       - Assign multiple relevant tags per pair from: поступление, документы, стипендия, общежитие, экзамены, специальности, дедлайны, требования, контакты, оплата.
                                       - Write all questions and answers in Russian.
                                       - Do not invent information not present in the document.
                                       - Extract all possible pairs — prefer many specific questions over few generic ones.
                                       - If a topic applies to a specific category (grant recipients, ex-military, foreign applicants) — mention that category in the answer.
                                       """
                    }
                });

            var mediaType = file.ContentType;
            var extractResponse = await agent.RunAsync(
            [
                new Microsoft.Extensions.AI.ChatMessage
                {
                    Role = ChatRole.User,
                    Contents =
                    [
                        new TextContent("Extract knowledge base Q&A pairs from this document."),
                        new DataContent(new ReadOnlyMemory<byte>(fileBytes), mediaType)
                    ]
                }
            ]);

            var result = extractResponse.Deserialize<KnowledgeQnAResult>(JsonSerializerOptions.Web);
            if (result is null) throw new Exception("GPT вернул пустой результат");

            foreach (var pair in result.Pairs)
            {
                await bus.PublishAsync(new EmbedKnowledgeEntry(
                    file.InstitutionId,
                    file.Id,
                    pair.Question,
                    pair.Answer,
                    pair.Tags
                ));
            }

            file.Status = KnowledgeFileStatus.Processed;
            file.ProcessedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            file.Status = KnowledgeFileStatus.Failed;
            file.ErrorMessage = ex.Message;
            await db.SaveChangesAsync();
        }
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