using Microsoft.Agents.AI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenAI;
using Talapker.Infrastructure.Data;
using Talapker.Infrastructure.Exceptions;

namespace Talapker.Application.AI.Talapker;

public interface ITalapkerAgent
{
    Task<TalapkerChatResponse> AskAsync(TalapkerChatRequest request, CancellationToken ct = default);
    Task AskStreamingAsync(
        TalapkerChatRequest request, 
        Func<StreamingChunk, Task> onChunk, 
        bool isFromTelegramBot = false,
        CancellationToken ct = default);
}

public class TalapkerAgent : ITalapkerAgent
{
    private readonly TalapkerDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TalapkerAgent> _logger;

    public TalapkerAgent(
        IConfiguration configuration,
        TalapkerDbContext dbContext,
        ILogger<TalapkerAgent> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<TalapkerChatResponse> AskAsync(
        TalapkerChatRequest request, 
        CancellationToken ct = default)
    {
        var fullResponse = string.Empty;
        
        await AskStreamingAsync(request, async chunk =>
        {
            if (!chunk.IsComplete && string.IsNullOrEmpty(chunk.Error))
            {
                fullResponse += chunk.Content;
            }
        }, false, ct);

        return new TalapkerChatResponse
        {
            Message = fullResponse,
            Timestamp = DateTime.UtcNow,
            UserId = request.UserId ?? Guid.Empty
        };
    }

    public async Task AskStreamingAsync(
        TalapkerChatRequest request, 
        Func<StreamingChunk, Task> onChunk, 
        bool isFromTelegramBot = false,
        CancellationToken ct = default)
    {
        try
        {
            var openAIClient = new OpenAIClient(_configuration["OpenAIKey"]!);
            
            var institution = await _dbContext.Institutions.AsNoTracking().FirstOrDefaultAsync(i => i.Id == request.InstitutionId, cancellationToken: ct);
            
            if (institution == null)
            {
                throw new DomainException("Institution not found", 500);
            }

            var educationPrograms = await _dbContext.EducationPrograms
                .AsNoTracking()
                .Include(ep => ep.Faculty)
                .Where(p => p.Faculty != null && p.Faculty.InstitutionId == request.InstitutionId)
                .Select(p => $"{p.Id} - {p.Code} - {p.Name.Ru}\n")
                .ToListAsync(ct);

            string instructions;

            if (isFromTelegramBot)
            {
                instructions = $@"
You are the official AI admissions consultant for {institution.Name} university. You help prospective students and their parents learn about the university, programs, and admission requirements.

AVAILABLE PROGRAMS (Id - Code - Name):
{string.Join("\n", educationPrograms)}

TOOLS:
- FindEducationProgram — fetch details for a program (description, UNT scores, internships, grants). Multiple Ids can be passed in one call.
- Do not call any tool if the answer is already in the conversation history or the program list above.

FORMAT: plain text only — no Markdown, no asterisks, no hashes, no bullet symbols. Paragraphs and numbered lists (1. 2. 3.) are fine.

LANGUAGE: always reply in the user's language (Kazakh / Russian / English). Never mention tools, databases, or system internals.

STRICTLY FORBIDDEN:
- Offering or promising help you cannot actually deliver right now
- If required data is not in context and no tool can fetch it — honestly refer the user to the admissions office
- Do not draft letters, statements, or documents
- Do not fabricate facts";
            }
            else
            {
                instructions = $@"
You are the official AI admissions consultant for {institution.Name} university. You help prospective students and their parents learn about the university, programs, and admission requirements.

AVAILABLE PROGRAMS (Id - Code - Name):
{string.Join("\n", educationPrograms)}

TOOLS:
- FindEducationProgram — fetch details for a program (description, UNT scores, internships, grants). Multiple Ids can be passed in one call.
- Do not call any tool if the answer is already in the conversation history or the program list above.

DIAGRAMS: when asked about grants, cutoff scores, or admission competition — embed the tag below (only for programs present in the list above):
{{grantStatisticDiagram:EDUCATION_PROGRAM_ID}}
Multiple tags can be used to compare programs side by side.

FORMAT: use Markdown — ## headings, **bold** for key data, lists, `program code` for codes. Short answers (1-2 facts) can be plain text.

LANGUAGE: always reply in the user's language (Kazakh / Russian / English). Never mention tools, databases, or system internals.

STRICTLY FORBIDDEN:
- Offering or promising help you cannot actually deliver right now
- If required data is not in context and no tool can fetch it — honestly refer the user to the admissions office
- Do not draft letters, statements, or documents
- Do not fabricate facts";
            }

            _logger.LogInformation(instructions);

            var tools = new UniversityTools(_dbContext);

            var findFunction = AIFunctionFactory.Create(
                tools.FindProgram,
                name: "FindEducationProgram",
                description: "Находит подробную информацию по специальностям: описание, места работы, базы практик, мин. балл ЕНТ, языки обучения и баллы для получения гранта. Передавай все нужные запросы сразу одним вызовом."
            );

            var options = new ChatClientAgentOptions
            {
                ChatMessageStoreFactory = (ctx) =>
                {
                    return new UserChatHistoryProvider(
                        _dbContext,
                        request.UserId ?? Guid.Empty,
                        request.InstitutionId, 
                        ctx.SerializedState,
                        ctx.JsonSerializerOptions);
                },
            
                ChatOptions = new ChatOptions 
                { 
                    Instructions = instructions, 
                    Tools = [findFunction]
                }
            };

            var agent = openAIClient
                .GetChatClient("gpt-5-nano-2025-08-07")
                .AsIChatClient()
                .CreateAIAgent(options);

            _logger.LogInformation($"Starting streaming for: {request.Message}");

            // Запускаем стриминг
            var response = agent.RunStreamingAsync(request.Message);

            // Отправляем чанки клиенту
            await foreach (var update in response)
            {
                await onChunk(new StreamingChunk
                {
                    Content = update.Text,
                    IsComplete = false
                });
            }

            // Отправляем сигнал о завершении
            await onChunk(new StreamingChunk
            {   
                IsComplete = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Streaming error");
            await onChunk(new StreamingChunk
            {
                Error = "Произошла ошибка при генерации ответа"
            });
            throw new DomainException("AI streaming error", 500);
        }
    }
}