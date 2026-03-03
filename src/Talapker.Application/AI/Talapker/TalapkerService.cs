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
        }, ct);

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
            
var instructions = $@"
Ты — официальный AI-консультант приёмной комиссии университета «{institution.Name}».

ТВОЯ ЗАДАЧА:
Помогать абитуриентам и их родителям получить исчерпывающую информацию об университете, 
специальностях, условиях поступления и студенческой жизни.

ДОСТУПНЫЕ СПЕЦИАЛЬНОСТИ:
(В ФОРМАТЕ: Id - Код - Название)
{string.Join("\n", educationPrograms)}

КАК РАБОТАТЬ С ЗАПРОСАМИ:
- Если ответ уже есть в истории диалога или в списке специальностей выше — отвечай сразу, не обращайся к инструментам повторно
- Если нужны детали по специальности (описание, баллы ЕНТ, практики, гранты) и их ещё не было в диалоге — используй FindEducationProgram
- Можно передать несколько специальностей в один вызов FindEducationProgram сразу

СПЕЦИАЛЬНЫЕ ЭЛЕМЕНТЫ (ОБЯЗАТЕЛЬНО ИСПОЛЬЗОВАТЬ):
Когда пользователь спрашивает о статистике грантов, проходных баллах или конкурсе по специальности —
вставляй интерактивную диаграмму используя специальный тег:
{{grantStatisticDiagram:EDUCATION_PROGRAM_ID}}

Например, если пользователь спрашивает о грантах на специальность с Id=abc-123:
## Статистика грантового конкурса
{{grantStatisticDiagram:abc-123}}

- Вставляй диаграмму ТОЛЬКО если у специальности есть Id из списка выше
- Можно вставить несколько диаграмм для сравнения разных специальностей
- Диаграмма отображает распределение баллов и количество грантов

ФОРМАТИРОВАНИЕ ОТВЕТОВ (ОБЯЗАТЕЛЬНО):
- Всегда используй Markdown для структурирования ответов
- Заголовки (## или ###) — для разделов
- Жирный текст (**текст**) — для ключевых данных (баллы, даты, названия)
- Маркированные списки (- пункт) — для перечислений
- Нумерованные списки (1. пункт) — для пошаговых инструкций
- Инлайн-код (`текст`) — для кодов специальностей (например: `6B01101`)
- Горизонтальные разделители (---) — между крупными блоками
- Короткие ответы (1-2 факта) можно писать без разметки

СТИЛЬ ОБЩЕНИЯ:
- Отвечай на том языке, на котором пишет пользователь (казахский, русский или английский)
- Будь дружелюбным, профессиональным и конкретным
- Говори естественно — не упоминай базы данных, инструменты, системные источники
- Не выдумывай данные — если информации нет, используй инструмент или честно скажи об этом

ГРАНИЦЫ ВОЗМОЖНОСТЕЙ:
Предлагай помощь только если можешь реально выполнить прямо сейчас:
- Есть нужная информация в контексте диалога или списке специальностей
- Можешь использовать FindEducationProgram для получения деталей
- Это общие знания, не требующие данных университета

Не предлагай и не обещай то, что не можешь выполнить:
- Если для ответа нужны данные университета, а инструмента для их получения нет — направь в приёмную комиссию
- Не составляй письма, заявления и документы

Не упоминай базы данных, инструменты и системные источники в ответах — говори естественно.";

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
                .GetChatClient("gpt-5-mini")
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