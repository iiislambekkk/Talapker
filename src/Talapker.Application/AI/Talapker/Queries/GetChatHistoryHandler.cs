using Microsoft.EntityFrameworkCore;
using Talapker.Infrastructure.Data;
using System.Text.Json;
using Talapker.Infrastructure.Data.Assistant;

namespace Talapker.Application.AI.Talapker.Queries;

public record GetChatHistoryQuery(
    Guid? UserId,
    Guid InstitutionId,
    DateTime? Cursor = null,
    int Limit = 30,
    bool Older = true
);
public record GetChatHistoryResponse(
    List<ChatMessageDto> Messages,
    DateTime? NextCursor,
    bool HasMore
);

public record ChatMessageDto
{
    public Guid Id { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Dictionary<string, object>? ToolCalls { get; set; }
    
    public bool IsUser => Role == "user";
    public bool IsAssistant => Role == "assistant";
    public string TimeAgo => GetTimeAgo(CreatedAt);
    
    private string GetTimeAgo(DateTime date)
    {
        var diff = DateTime.UtcNow - date;
        return diff.TotalMinutes switch
        {
            < 1 => "только что",
            < 60 => $"{diff.Minutes} мин назад",
            < 120 => "час назад",
            < 24 * 60 => $"{diff.Hours} ч назад",
            _ => $"{diff.Days} д назад"
        };
    }
}

public class GetChatHistoryHandler
{
    public async Task<GetChatHistoryResponse> Handle(
        GetChatHistoryQuery query,
        TalapkerDbContext db,
        CancellationToken cancellationToken)
    {
        // Валидация
        if (!query.UserId.HasValue)
            return new GetChatHistoryResponse([], null, false);

        // Базовый запрос
        var baseQuery = db.AssistantChatMessage
            .AsNoTracking()
            .Where(m => !string.IsNullOrWhiteSpace(m.Content))
            .Where(m => m.UserId == query.UserId && m.InstitutionId == query.InstitutionId);

        // Применяем курсорную пагинацию
        IQueryable<AssistantChatMessage> pagedQuery;
        
        if (query.Cursor.HasValue)
        {
            if (query.Older)
            {
                // Загружаем сообщения СТАРШЕ курсора (более старые)
                pagedQuery = baseQuery
                    .Where(m => m.CreatedAt < query.Cursor.Value)
                    .OrderByDescending(m => m.CreatedAt); // Сортируем от новых к старым внутри страницы
            }
            else
            {
                // Загружаем сообщения НОВЕЕ курсора (более новые)
                pagedQuery = baseQuery
                    .Where(m => m.CreatedAt > query.Cursor.Value)
                    .OrderBy(m => m.CreatedAt); // Сортируем от старых к новым внутри страницы
            }
        }
        else
        {
            // Первая загрузка - самые новые сообщения (снизу)
            pagedQuery = baseQuery
                .OrderByDescending(m => m.CreatedAt); // Сначала новые, потом старые
        }

        // Получаем на одно сообщение больше для определения hasMore
        var messages = await pagedQuery
            .Take(query.Limit + 1)
            .ToListAsync(cancellationToken);

        // Проверяем, есть ли еще сообщения
        var hasMore = messages.Count > query.Limit;
        
        // Убираем лишнее сообщение
        if (hasMore)
            messages.RemoveAt(messages.Count - 1);

        // Определяем следующий курсор в зависимости от направления
        DateTime? nextCursor = null;
        
        if (messages.Any())
        {
            if (query.Older || !query.Cursor.HasValue)
            {
                // При загрузке старых сообщений или первой загрузке
                // Следующий курсор - дата САМОГО СТАРОГО сообщения в текущей странице
                // (последнее в списке после сортировки)
                nextCursor = messages.Last().CreatedAt;
            }
            else
            {
                // При загрузке новых сообщений
                // Следующий курсор - дата САМОГО НОВОГО сообщения в текущей странице
                // (первое в списке после сортировки)
                nextCursor = messages.First().CreatedAt;
            }
        }

        // Конвертируем в DTO и сортируем для фронтенда (от старых к новым сверху вниз)
        var messageDtos = messages
            .Select(m => new ChatMessageDto
            {
                Id = m.Id,
                Role = m.Role,
                Content = m.Content,
                CreatedAt = m.CreatedAt,
                ToolCalls = m.ToolCalls != null 
                    ? JsonSerializer.Deserialize<Dictionary<string, object>>(m.ToolCalls)
                    : null
            })
            .OrderBy(m => m.CreatedAt) // Важно! Сортируем от старых к новым для правильного отображения
            .ToList();

        // Логируем для отладки
        Console.WriteLine($"[GetChatHistory] UserId: {query.UserId}, Cursor: {query.Cursor}, Older: {query.Older}");
        Console.WriteLine($"[GetChatHistory] Found {messageDtos.Count} messages, HasMore: {hasMore}, NextCursor: {nextCursor}");
        if (messageDtos.Any())
        {
            Console.WriteLine($"[GetChatHistory] First message date: {messageDtos.First().CreatedAt}, Last message date: {messageDtos.Last().CreatedAt}");
        }

        return new GetChatHistoryResponse(messageDtos, nextCursor, hasMore);
    }
}