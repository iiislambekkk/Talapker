using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Talapker.Infrastructure.Data;
using Talapker.Infrastructure.Data.Assistant;

namespace Talapker.Application.AI.Talapker;

public class UserChatHistoryProvider : ChatMessageStore
{
    private readonly TalapkerDbContext _dbContext;
    private Guid _userId;
    private Guid _institutionId;
    private readonly int _maxHistoryMessages = 10;
    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = false };
    private readonly ILogger<UserChatHistoryProvider> _logger;

    public UserChatHistoryProvider(
        TalapkerDbContext dbContext,
        Guid userId,
        Guid institutionId,
        JsonElement serializedStoreState,
        JsonSerializerOptions? jsonSerializerOptions = null,
        ILogger<UserChatHistoryProvider>? logger = null)
    {
        _dbContext = dbContext;
        _userId = userId;
        _institutionId = institutionId;
        _logger = logger;
    }

    public override async Task<IEnumerable<ChatMessage>> GetMessagesAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (_userId == Guid.Empty)
                return Enumerable.Empty<ChatMessage>();

            var messages = await _dbContext.AssistantChatMessage
                .Where(m => m.UserId == _userId && m.InstitutionId == _institutionId)
                .OrderByDescending(m => m.CreatedAt)
                .Take(_maxHistoryMessages)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync(cancellationToken);

            return messages.Select(m =>
            {
                var message = new ChatMessage(role: ParseRole(m.Role), content: m.Content);
                message.CreatedAt = DateTime.SpecifyKind(m.CreatedAt, DateTimeKind.Utc);
                return message;
            });
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "[GetMessagesAsync] Error");
            return Enumerable.Empty<ChatMessage>();
        }
    }

    public override async Task AddMessagesAsync(
        IEnumerable<ChatMessage> messages,
        CancellationToken cancellationToken = default)
    {
        if (_userId == Guid.Empty)
            return;

        var historyItems = messages.Select(msg => new AssistantChatMessage
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            InstitutionId = _institutionId,
            Role = msg.Role.ToString(),
            Content = msg.Text ?? string.Empty,
            CreatedAt = DateTime.UtcNow,
            ToolCalls = SerializeToolCalls(msg.Contents)
        });

        await _dbContext.AssistantChatMessage.AddRangeAsync(historyItems, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public override JsonElement Serialize(JsonSerializerOptions? jsonSerializerOptions = null)
        => JsonSerializer.SerializeToElement(_userId.ToString());

    private string? SerializeToolCalls(IList<AIContent>? contents)
    {
        if (contents == null) return null;
        var toolCalls = contents.OfType<FunctionCallContent>().ToList();
        return toolCalls.Any() ? JsonSerializer.Serialize(toolCalls, _jsonOptions) : null;
    }

    private static ChatRole ParseRole(string role) => role?.ToLower() switch
    {
        "user" => ChatRole.User,
        "assistant" => ChatRole.Assistant,
        "system" => ChatRole.System,
        "tool" => ChatRole.Tool,
        _ => ChatRole.User
    };
}