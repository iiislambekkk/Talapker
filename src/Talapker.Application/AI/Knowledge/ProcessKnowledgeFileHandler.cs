using System.ClientModel;
using System.Text.Json;
using Amazon.S3;
using Amazon.S3.Model;
using Docnet.Core;
using Docnet.Core.Models;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using Microsoft.Agents.AI;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenAI;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using Talapker.Infrastructure.Data;
using Talapker.Infrastructure.Data.Assistant;
using Talapker.Notifications.Contracts;
using Wolverine;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;
using Image = SixLabors.ImageSharp.Image;

namespace Talapker.Application.AI.Knowledge;

public record KnowledgeQnAPair(string Question, string Answer, List<string> Tags);
public record KnowledgeQnAResult(List<KnowledgeQnAPair> Pairs);

public class ProcessKnowledgeFileHandler
{
    private const string QdrantCollection = "knowledge";
    private const int VectorSize = 1536;
    private const int PagesPerChunk = 8;
    private const int Overlap = 2;
    private const int VisionBatchSize = 4;
    private const int RenderScale = 2;

    private const string BaseInstructions = """
        You are building a RAG (Retrieval-Augmented Generation) knowledge base for a university admissions AI chatbot.

        This chatbot answers questions from prospective students (абитуриенты) in Kazakhstan who know nothing about university rules, 
        documents, deadlines, or procedures. They will ask vague, natural questions like:
        - "как поступить?"
        - "что нужно для поступления?"
        - "какие документы готовить?"
        - "когда подавать документы?"

        Your job is to create Q&A pairs that will be retrieved via semantic search when a student asks such questions.
        This means:

        QUESTION QUALITY RULES:
        - Write questions the way a real student would type them — natural, conversational, sometimes vague
        - Generate MULTIPLE question variants for the same answer when possible:
          "Какие документы нужны для поступления?" AND "Что подготовить абитуриенту?" AND "Список документов для подачи" — all for the same answer
        - For category-specific info (иностранцы, льготники, военные, грантники) — include the category explicitly in the question:
          "Какие документы нужны иностранному гражданину для поступления?" not "Какие документы нужны?"
        - Mix specific and general question formulations so both precise and vague searches find the right answer

        ANSWER QUALITY RULES:
        - Answers must be 100% self-contained — the student should get everything they need without reading any document
        - Never say "см. таблицу", "см. раздел", "указано выше" — embed the actual data directly
        - If a rule applies only to a specific group — state that group at the start of the answer:
          "Для иностранных граждан: ..." or "Грантополучателям необходимо..."
        - Include all relevant details: deadlines, addresses, emails, phone numbers, exact document names, form numbers
        - Write in clear, friendly Russian — not bureaucratic language

        TAGS:
        - Assign multiple tags from: поступление, документы, стипендия, общежитие, экзамены, специальности, дедлайны, требования, контакты, оплата, иностранцы, льготы, гранты
        - Add context-specific tags like "иностранцы" when relevant

        COVERAGE:
        - Extract ALL possible pairs — every rule, deadline, requirement, contact, condition
        - Do NOT invent anything not present in the source
        """;

    public async Task Handle(
        ProcessKnowledgeFile message,
        TalapkerDbContext db,
        IAmazonS3 s3Client,
        IConfiguration configuration,
        IHubContext<KnowledgeHub> hubContext,
        IMessageBus bus,
        ILogger<ProcessKnowledgeFileHandler> logger)
    {
        logger.LogInformation("[KnowledgeFile:{FileId}] Handler started", message.FileId);

        var file = await db.KnowledgeFiles.FindAsync(message.FileId);
        if (file is null)
        {
            logger.LogWarning("[KnowledgeFile:{FileId}] Not found in DB", message.FileId);
            return;
        }

        logger.LogInformation("[KnowledgeFile:{FileId}] File: {FileName}, ContentType: {ContentType}, HasAdditionalText: {HasText}",
            file.Id, file.FileName, file.ContentType, !string.IsNullOrWhiteSpace(message.AdditionalText));

        file.Status = KnowledgeFileStatus.Processing;
        await db.SaveChangesAsync();

        await hubContext.Clients
            .Group($"institution:{file.InstitutionId}")
            .SendAsync("FileStatusChanged", new
            {
                fileId = file.Id,
                status = file.Status.ToString(),
                entriesCount = 0
            });

        try
        {
            var openAIClient = new OpenAIClient(
                new ApiKeyCredential(configuration["OpenAIKey"]!),
                new OpenAIClientOptions { NetworkTimeout = TimeSpan.FromMinutes(10) }
            );

            var schema = AIJsonUtilities.CreateJsonSchema(typeof(KnowledgeQnAResult));
            var chatOptions = new ChatOptions
            {
                ResponseFormat = ChatResponseFormat.ForJsonSchema(
                    schema: schema,
                    schemaName: "KnowledgeQnAResult",
                    schemaDescription: "List of question-answer pairs for a university admissions RAG chatbot"),
                Instructions = BaseInstructions
            };

            var agent = openAIClient
                .GetChatClient("gpt-5.4-mini")
                .AsIChatClient()
                .CreateAIAgent(new ChatClientAgentOptions { ChatOptions = chatOptions });

            var allPairs = new List<KnowledgeQnAPair>();
            var additionalText = message.AdditionalText;

            if (string.IsNullOrEmpty(file.StorageKey))
            {
                logger.LogInformation("[KnowledgeFile:{FileId}] Text-only mode", file.Id);
                await ProcessTextAsync(file.Id, additionalText!, agent, allPairs, logger);
            }
            else
            {
                logger.LogInformation("[KnowledgeFile:{FileId}] Downloading from R2, key: {Key}", file.Id, file.StorageKey);
                var s3Response = await s3Client.GetObjectAsync(new GetObjectRequest
                {
                    BucketName = configuration["AWS3Settings:Bucket"]!,
                    Key = file.StorageKey
                });

                using var memoryStream = new MemoryStream();
                await s3Response.ResponseStream.CopyToAsync(memoryStream);
                var fileBytes = memoryStream.ToArray();
                logger.LogInformation("[KnowledgeFile:{FileId}] Downloaded {Size} bytes", file.Id, fileBytes.Length);

                if (file.ContentType == "application/pdf")
                {
                    logger.LogInformation("[KnowledgeFile:{FileId}] Processing as PDF", file.Id);
                    await ProcessPdfAsync(file.Id, fileBytes, additionalText, agent, allPairs, logger);
                }
                else if (file.ContentType.StartsWith("image/"))
                {
                    logger.LogInformation("[KnowledgeFile:{FileId}] Processing as image via Vision", file.Id);
                    await ProcessImageAsync(file.Id, fileBytes, file.ContentType, additionalText, agent, allPairs, logger);
                }
                else if (file.ContentType == "text/plain")
                {
                    logger.LogInformation("[KnowledgeFile:{FileId}] Processing as plain text", file.Id);
                    var fileText = System.Text.Encoding.UTF8.GetString(fileBytes);
                    var combined = string.IsNullOrWhiteSpace(additionalText)
                        ? fileText
                        : $"{fileText}\n\n---\nАдминистратор уточняет:\n{additionalText}";
                    await ProcessTextAsync(file.Id, combined, agent, allPairs, logger);
                }
                else
                {
                    logger.LogWarning("[KnowledgeFile:{FileId}] Unsupported type {Type}, Vision fallback", file.Id, file.ContentType);
                    await ProcessImageAsync(file.Id, fileBytes, file.ContentType, additionalText, agent, allPairs, logger);
                }
            }

            logger.LogInformation("[KnowledgeFile:{FileId}] Total pairs extracted: {Count}", file.Id, allPairs.Count);

            var qdrant = new QdrantClient(
                configuration["Qdrant:Host"] ?? "localhost",
                int.Parse(configuration["Qdrant:Port"] ?? "6334")
            );
            await EnsureCollectionAsync(qdrant);

            var embeddingClient = openAIClient.GetEmbeddingClient("text-embedding-3-small");

            logger.LogInformation("[KnowledgeFile:{FileId}] Starting embedding for {Count} pairs", file.Id, allPairs.Count);

            foreach (var (pair, pairIndex) in allPairs.Select((p, i) => (p, i)))
            {
                logger.LogDebug("[KnowledgeFile:{FileId}] Embedding {Index}/{Total}: {Question}",
                    file.Id, pairIndex + 1, allPairs.Count, pair.Question);

                var sw = System.Diagnostics.Stopwatch.StartNew();
                var embeddingResult = await embeddingClient.GenerateEmbeddingAsync(pair.Question);
                var embedding = embeddingResult.Value.ToFloats().ToArray();
                sw.Stop();

                logger.LogDebug("[KnowledgeFile:{FileId}] Embedding {Index}/{Total} done in {Elapsed}ms",
                    file.Id, pairIndex + 1, allPairs.Count, sw.ElapsedMilliseconds);

                var entry = new KnowledgeEntry
                {
                    InstitutionId = file.InstitutionId,
                    SourceFileId = file.Id,
                    Question = pair.Question,
                    Answer = pair.Answer,
                    Tags = pair.Tags.ToArray()
                };

                db.KnowledgeEntries.Add(entry);
                await db.SaveChangesAsync();

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
                            ["source_file_id"] = file.Id.ToString(),
                            ["source_file_name"] = file.FileName,
                            ["source_file_key"] = file.StorageKey
                        }
                    }
                ]);

                if ((pairIndex + 1) % 10 == 0)
                    logger.LogInformation("[KnowledgeFile:{FileId}] Progress: {Done}/{Total} pairs saved",
                        file.Id, pairIndex + 1, allPairs.Count);
            }

            file.Status = KnowledgeFileStatus.Processed;
            file.ProcessedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();

            await hubContext.Clients
                .Group($"institution:{file.InstitutionId}")
                .SendAsync("FileStatusChanged", new
                {
                    fileId = file.Id,
                    status = file.Status.ToString(),
                    entriesCount = allPairs.Count
                });

            logger.LogInformation("[KnowledgeFile:{FileId}] ✓ Done: {FileName}, {Count} pairs",
                file.Id, file.FileName, allPairs.Count);

            if (file.UploadedById.HasValue)
            {
                logger.LogInformation("[KnowledgeFile:{FileId}] Sending push to user {UserId}", file.Id, file.UploadedById.Value);

                await bus.PublishAsync(new SendPushCommand(
                    UserId: file.UploadedById.Value,
                    Title: "Knowledge base updated",
                    Body: $"File \"{file.FileName}\" processed successfully — {allPairs.Count} Q&A pairs added.",
                    Type: "KnowledgeFileProcessed"
                ));

                logger.LogInformation("[KnowledgeFile:{FileId}] Push sent to user {UserId}", file.Id, file.UploadedById.Value);

                await hubContext.Clients
                    .Group($"institution:{file.InstitutionId}")
                    .SendAsync("KnowledgeFileNotification", new
                    {
                        type = "success",
                        title = "База знаний обновлена",
                        message = $"Файл \"{file.FileName}\" обработан — добавлено {allPairs.Count} пар вопрос-ответ.",
                        fileId = file.Id,
                    });
            }
            else
            {
                logger.LogWarning("[KnowledgeFile:{FileId}] UploadedById is null, skipping push", file.Id);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[KnowledgeFile:{FileId}] ✗ Failed: {Message}", file.Id, ex.Message);
            file.Status = KnowledgeFileStatus.Failed;
            file.ErrorMessage = ex.Message;
            await db.SaveChangesAsync();

            await hubContext.Clients
                .Group($"institution:{file.InstitutionId}")
                .SendAsync("FileStatusChanged", new
                {
                    fileId = file.Id,
                    status = file.Status.ToString(),
                    errorMessage = ex.Message
                });
        }
    }

    // ── PDF processing ────────────────────────────────────────────────────────

    private async Task ProcessPdfAsync(
        Guid fileId,
        byte[] fileBytes,
        string? additionalText,
        AIAgent agent,
        List<KnowledgeQnAPair> allPairs,
        ILogger logger)
    {
        var pages = ExtractPdfPages(fileBytes);
        logger.LogInformation("[KnowledgeFile:{FileId}] PDF has {PageCount} text pages", fileId, pages.Count);

        // Отсканированный PDF — текст не извлёкся, переключаемся на Vision
        if (pages.Count == 0 || pages.All(p => string.IsNullOrWhiteSpace(p)))
        {
            logger.LogInformation("[KnowledgeFile:{FileId}] Scanned PDF detected, rendering pages as images", fileId);
            var pageImages = RenderPdfPagesAsImages(fileBytes, fileId, logger);
            logger.LogInformation("[KnowledgeFile:{FileId}] Rendered {Count} pages", fileId, pageImages.Count);
            await ProcessScannedPdfAsync(fileId, pageImages, additionalText, agent, allPairs, logger);
            return;
        }

        var fullText = string.Join("\n\n", pages);
        var adminContext = BuildAdminContext(additionalText);

        // Pass 1 — глобальный
        logger.LogInformation("[KnowledgeFile:{FileId}] Starting global pass ({Chars} chars)", fileId, fullText.Length);
        var globalSw = System.Diagnostics.Stopwatch.StartNew();

        var globalResponse = await agent.RunAsync(
        [
            new ChatMessage
            {
                Role = ChatRole.User,
                Contents =
                [
                    new TextContent($"""
                        {adminContext}

                        SOURCE DOCUMENT (full text for global pass):
                        {fullText}

                        YOUR TASK — GLOBAL PASS:
                        Focus on extracting facts that are referenced from multiple places in the document:
                        - Tables with structured data (deadlines, score conversions, fees, grade mappings) — embed the actual values
                        - Contact info (emails, phones, addresses, office hours)
                        - Consolidated lists of requirements
                        - Appendix/annex data

                        For each fact, generate multiple question variants a student might ask to find it.
                        """)
                ]
            }
        ]);
        globalSw.Stop();

        logger.LogInformation("[KnowledgeFile:{FileId}] Global pass done in {Elapsed}ms, raw length: {Len}",
            fileId, globalSw.ElapsedMilliseconds, globalResponse.Text?.Length ?? 0);
        logger.LogDebug("[KnowledgeFile:{FileId}] Global raw: {Response}", fileId, globalResponse.Text);

        var globalResult = globalResponse.Deserialize<KnowledgeQnAResult>(JsonSerializerOptions.Web);
        if (globalResult is not null)
        {
            allPairs.AddRange(globalResult.Pairs);
            logger.LogInformation("[KnowledgeFile:{FileId}] Global pass produced {Count} pairs", fileId, globalResult.Pairs.Count);
        }
        else
        {
            logger.LogWarning("[KnowledgeFile:{FileId}] Global pass returned null", fileId);
        }

        var chunks = SplitIntoChunks(pages, PagesPerChunk, Overlap);

        if (chunks.Count <= 1)
        {
            logger.LogInformation("[KnowledgeFile:{FileId}] Short document ({PageCount} pages), skipping chunk pass",
                fileId, pages.Count);
            return;
        }

        var globalContext = string.Join("\n", (globalResult?.Pairs ?? [])
            .Select(p => $"Q: {p.Question}\nA: {p.Answer}"));

        logger.LogInformation("[KnowledgeFile:{FileId}] Starting chunk pass: {Count} chunks", fileId, chunks.Count);

        foreach (var (chunk, index) in chunks.Select((c, i) => (c, i)))
        {
            logger.LogInformation("[KnowledgeFile:{FileId}] Chunk {Index}/{Total} ({Chars} chars)...",
                fileId, index + 1, chunks.Count, chunk.Length);

            var chunkSw = System.Diagnostics.Stopwatch.StartNew();
            var chunkResponse = await agent.RunAsync(
            [
                new ChatMessage
                {
                    Role = ChatRole.User,
                    Contents =
                    [
                        new TextContent($"""
                            {adminContext}

                            GLOBAL REFERENCE DATA (already extracted from full document — use to complete answers that say "see table" or "see section"):
                            {globalContext}

                            ---
                            CHUNK {index + 1}/{chunks.Count}:
                            {chunk}

                            YOUR TASK:
                            Extract ALL Q&A pairs from this chunk.
                            - Use global reference data to fill in any values referenced but not present in this chunk
                            - Generate multiple question variants per answer when the topic could be searched different ways
                            - Be exhaustive — every rule, condition, deadline, procedure must become at least one pair
                            """)
                    ]
                }
            ]);
            chunkSw.Stop();

            logger.LogInformation("[KnowledgeFile:{FileId}] Chunk {Index}/{Total} done in {Elapsed}ms, raw length: {Len}",
                fileId, index + 1, chunks.Count, chunkSw.ElapsedMilliseconds, chunkResponse.Text?.Length ?? 0);
            logger.LogDebug("[KnowledgeFile:{FileId}] Chunk {Index} raw: {Response}", fileId, index + 1, chunkResponse.Text);

            var chunkResult = chunkResponse.Deserialize<KnowledgeQnAResult>(JsonSerializerOptions.Web);
            if (chunkResult is not null)
            {
                allPairs.AddRange(chunkResult.Pairs);
                logger.LogInformation("[KnowledgeFile:{FileId}] Chunk {Index}/{Total} → {Count} pairs, total: {RunningTotal}",
                    fileId, index + 1, chunks.Count, chunkResult.Pairs.Count, allPairs.Count);
            }
            else
            {
                logger.LogWarning("[KnowledgeFile:{FileId}] Chunk {Index}/{Total} returned null", fileId, index + 1, chunks.Count);
            }
        }
    }

    private static async Task ProcessScannedPdfAsync(
        Guid fileId,
        List<byte[]> pageImages,
        string? additionalText,
        AIAgent agent,
        List<KnowledgeQnAPair> allPairs,
        ILogger logger)
    {
        var adminContext = BuildAdminContext(additionalText);
        var totalBatches = (int)Math.Ceiling(pageImages.Count / (double)VisionBatchSize);

        for (int i = 0; i < pageImages.Count; i += VisionBatchSize)
        {
            var batch = pageImages.Skip(i).Take(VisionBatchSize).ToList();
            var batchNum = i / VisionBatchSize + 1;

            logger.LogInformation("[KnowledgeFile:{FileId}] Vision batch {Batch}/{Total} (pages {From}–{To})",
                fileId, batchNum, totalBatches, i + 1, i + batch.Count);

            var contents = new List<AIContent>
            {
                new TextContent($"""
                    {adminContext}

                    These are pages {i + 1}–{i + batch.Count} of a scanned PDF document.
                    Extract ALL possible Q&A pairs. Generate multiple question variants per answer.
                    Be exhaustive — every rule, deadline, requirement, contact must become at least one pair.
                    """)
            };

            foreach (var img in batch)
                contents.Add(new DataContent(new ReadOnlyMemory<byte>(img), "image/png"));

            var sw = System.Diagnostics.Stopwatch.StartNew();
            var response = await agent.RunAsync(
            [
                new ChatMessage { Role = ChatRole.User, Contents = contents }
            ]);
            sw.Stop();

            logger.LogInformation("[KnowledgeFile:{FileId}] Vision batch {Batch}/{Total} done in {Elapsed}ms, raw length: {Len}",
                fileId, batchNum, totalBatches, sw.ElapsedMilliseconds, response.Text?.Length ?? 0);
            logger.LogDebug("[KnowledgeFile:{FileId}] Vision batch {Batch} raw: {Response}", fileId, batchNum, response.Text);

            var result = response.Deserialize<KnowledgeQnAResult>(JsonSerializerOptions.Web);
            if (result is not null)
            {
                allPairs.AddRange(result.Pairs);
                logger.LogInformation("[KnowledgeFile:{FileId}] Vision batch {Batch}/{Total} → {Count} pairs, total: {Total2}",
                    fileId, batchNum, totalBatches, result.Pairs.Count, allPairs.Count);
            }
            else
            {
                logger.LogWarning("[KnowledgeFile:{FileId}] Vision batch {Batch}/{Total} returned null", fileId, batchNum, totalBatches);
            }
        }
    }

    // ── Image processing ──────────────────────────────────────────────────────

    private static async Task ProcessImageAsync(
        Guid fileId,
        byte[] fileBytes,
        string contentType,
        string? additionalText,
        AIAgent agent,
        List<KnowledgeQnAPair> allPairs,
        ILogger logger)
    {
        logger.LogInformation("[KnowledgeFile:{FileId}] Vision pass ({Bytes} bytes)", fileId, fileBytes.Length);
        var sw = System.Diagnostics.Stopwatch.StartNew();

        var adminContext = BuildAdminContext(additionalText);

        var response = await agent.RunAsync(
        [
            new ChatMessage
            {
                Role = ChatRole.User,
                Contents =
                [
                    new TextContent($"""
                        {adminContext}

                        Extract ALL possible Q&A pairs from this image.
                        Generate multiple question variants per answer — vague ones like "что нужно?" AND specific ones.
                        Be exhaustive.
                        """),
                    new DataContent(new ReadOnlyMemory<byte>(fileBytes), contentType)
                ]
            }
        ]);
        sw.Stop();

        logger.LogInformation("[KnowledgeFile:{FileId}] Vision done in {Elapsed}ms, raw length: {Len}",
            fileId, sw.ElapsedMilliseconds, response.Text?.Length ?? 0);
        logger.LogDebug("[KnowledgeFile:{FileId}] Vision raw: {Response}", fileId, response.Text);

        var result = response.Deserialize<KnowledgeQnAResult>(JsonSerializerOptions.Web);
        if (result is not null)
        {
            allPairs.AddRange(result.Pairs);
            logger.LogInformation("[KnowledgeFile:{FileId}] Vision produced {Count} pairs", fileId, result.Pairs.Count);
        }
        else
        {
            logger.LogWarning("[KnowledgeFile:{FileId}] Vision returned null", fileId);
        }
    }

    // ── Text processing ───────────────────────────────────────────────────────

    private static async Task ProcessTextAsync(
        Guid fileId,
        string text,
        AIAgent agent,
        List<KnowledgeQnAPair> allPairs,
        ILogger logger)
    {
        logger.LogInformation("[KnowledgeFile:{FileId}] Text pass ({Chars} chars)", fileId, text.Length);
        var sw = System.Diagnostics.Stopwatch.StartNew();

        var response = await agent.RunAsync(
        [
            new ChatMessage
            {
                Role = ChatRole.User,
                Contents =
                [
                    new TextContent($"""
                        Extract ALL possible Q&A pairs from the text below.
                        Generate multiple question variants per answer — both vague and specific.
                        Be exhaustive.

                        TEXT:
                        {text}
                        """)
                ]
            }
        ]);
        sw.Stop();

        logger.LogInformation("[KnowledgeFile:{FileId}] Text pass done in {Elapsed}ms, raw length: {Len}",
            fileId, sw.ElapsedMilliseconds, response.Text?.Length ?? 0);
        logger.LogDebug("[KnowledgeFile:{FileId}] Text raw: {Response}", fileId, response.Text);

        var result = response.Deserialize<KnowledgeQnAResult>(JsonSerializerOptions.Web);
        if (result is not null)
        {
            allPairs.AddRange(result.Pairs);
            logger.LogInformation("[KnowledgeFile:{FileId}] Text pass produced {Count} pairs", fileId, result.Pairs.Count);
        }
        else
        {
            logger.LogWarning("[KnowledgeFile:{FileId}] Text pass returned null", fileId);
        }
    }

    // ── Rendering ─────────────────────────────────────────────────────────────

    private static List<byte[]> RenderPdfPagesAsImages(byte[] pdfBytes, Guid fileId, ILogger logger)
    {
        var images = new List<byte[]>();

        using var lib = DocLib.Instance;
        using var docReader = lib.GetDocReader(pdfBytes, new PageDimensions(RenderScale));

        var pageCount = docReader.GetPageCount();
        logger.LogInformation("[KnowledgeFile:{FileId}] Rendering {Count} pages at {Scale}x", fileId, pageCount, RenderScale);

        for (int i = 0; i < pageCount; i++)
        {
            using var pageReader = docReader.GetPageReader(i);

            var width  = pageReader.GetPageWidth();
            var height = pageReader.GetPageHeight();
            var rawBytes = pageReader.GetImage(); // BGRA

            using var image = Image.LoadPixelData<Bgra32>(rawBytes, width, height);
            using var ms = new MemoryStream();
            image.Save(ms, new PngEncoder());
            images.Add(ms.ToArray());

            logger.LogDebug("[KnowledgeFile:{FileId}] Page {Page}/{Total} rendered ({W}x{H})",
                fileId, i + 1, pageCount, width, height);
        }

        return images;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static List<string> ExtractPdfPages(byte[] pdfBytes)
    {
        var pages = new List<string>();
        using var stream = new MemoryStream(pdfBytes);
        using var reader = new PdfReader(stream);
        using var doc = new PdfDocument(reader);

        for (int i = 1; i <= doc.GetNumberOfPages(); i++)
        {
            var text = PdfTextExtractor.GetTextFromPage(doc.GetPage(i));
            if (!string.IsNullOrWhiteSpace(text))
                pages.Add(text);
        }

        return pages;
    }

    private static List<string> SplitIntoChunks(List<string> pages, int pagesPerChunk, int overlap)
    {
        var chunks = new List<string>();
        var step = pagesPerChunk - overlap;

        for (int i = 0; i < pages.Count; i += step)
        {
            var chunk = pages.Skip(i).Take(pagesPerChunk);
            chunks.Add(string.Join("\n\n", chunk));
            if (i + pagesPerChunk >= pages.Count) break;
        }

        return chunks;
    }

    private static string BuildAdminContext(string? additionalText) =>
        string.IsNullOrWhiteSpace(additionalText)
            ? string.Empty
            : $"""
               ⚠️ ADMINISTRATOR CONTEXT — CRITICAL, apply to every Q&A pair:
               {additionalText}

               This context MUST be reflected in every question and answer where relevant.
               Do NOT generate generic questions that ignore this context.
               If this context restricts the audience (e.g. "only for foreign citizens") — 
               every question must explicitly name that audience.

               ---
               """;

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