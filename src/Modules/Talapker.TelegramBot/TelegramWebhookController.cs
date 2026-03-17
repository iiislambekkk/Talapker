using System.Text;
using System.Text.Json;
using Marten;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Talapker.Application;
using Talapker.Application.AI.Talapker;
using Talapker.Infrastructure.Vault;
using Talapker.TelegramBot.Features;
using Talapker.TelegramBot.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Wolverine;

namespace Talapker.TelegramBot;

[ApiController]
[Route("api/telegram")]
public class TelegramWebhookController : ControllerBase
{  
    private readonly IDocumentSession _session;
    private readonly IVaultStore _vaultStore;
    private readonly IMessageBus _messageBus;
    private readonly ITalapkerAgent _agent;
    private readonly TelegramClientFactory _clientFactory;
    private readonly ILogger<TelegramWebhookController> _logger;

    public TelegramWebhookController(
        IDocumentSession session,
        ITalapkerAgent agent,
        TelegramClientFactory clientFactory,
        ILogger<TelegramWebhookController> logger,
        IVaultStore vaultStore,
        IMessageBus messageBus
        )
    {
        _session = session;
        _agent = agent;
        _clientFactory = clientFactory;
        _logger = logger;
        _messageBus = messageBus;
        _vaultStore = vaultStore;
    }
    
    [HttpGet("institutions/{institutionId:guid}")]
    public async Task<IActionResult> GetBot(Guid institutionId, [FromServices] IMessageBus messageBus)
    {
        var result = await messageBus.InvokeAsync<GetTelegramBotDto?>(new GetTelegramBotQuery(institutionId));
        return result is null ? NotFound() : Ok(result);
    }
    
    [HttpPost]  
    public async Task<IActionResult> AddBot([FromBody] AddTelegramBotCommand command)
    {
        var result = await _messageBus.InvokeAsync<ApiResponse>(command);
        return result.IsSuccess ? Ok() : BadRequest(result);
    }
    
    // GET /api/telegram/admin/institutions/{institutionId}/users?page=1&pageSize=20
    [HttpGet("admin/institutions/{institutionId:guid}/users")]
    public async Task<IActionResult> GetBotUsers(
        Guid institutionId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        var result = await _messageBus.InvokeAsync<PagedResult<TelegramBotUserDto>>(
            new GetTelegramBotUsersQuery(institutionId, Math.Max(1, page), pageSize));
        return Ok(result);
    }

// GET /api/telegram/admin/institutions/{institutionId}/users/{telegramUserId}/history
    [HttpGet("admin/institutions/{institutionId:guid}/users/{telegramUserId:long}/history")]
    public async Task<IActionResult> GetUserHistory(Guid institutionId, long telegramUserId)
    {
        var result = await _messageBus.InvokeAsync<TelegramUserHistoryDto?>(
            new GetTelegramUserHistoryQuery(institutionId, telegramUserId));
        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("{botId:guid}")]
    public async Task<IActionResult> RemoveBot(Guid botId, [FromQuery] Guid institutionId)
    {
        var result = await _messageBus.InvokeAsync<ApiResponse>(new RemoveTelegramBotCommand(botId, institutionId));
        return result.IsSuccess ? Ok() : BadRequest(result);
    }

    
    [HttpPost("webhook/{botId:guid}")]
    public async Task<IActionResult> Webhook(Guid botId)
    {
        Update update;
        try
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();
            update = JsonSerializer.Deserialize<Update>(body, JsonBotAPI.Options)!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Telegram] Failed to deserialize update for bot {BotId}", botId);
            return Ok();
        }

        var bot = await _session.Query<Models.TelegramBot>()
            .FirstOrDefaultAsync(b => b.Id == botId && b.IsActive);

        if (bot is null)
        {
            _logger.LogWarning("[Telegram] Unknown or inactive bot: {BotId}", botId);
            return Ok();
        }

        if (update.Type != UpdateType.Message || update.Message?.Text is null)
            return Ok();
        
        var message = update.Message;
        var token = await _vaultStore.ReadTokenAsync(botId, HttpContext.RequestAborted);
        var botClient = _clientFactory.GetClient(token ?? "");
        
        _logger.LogInformation("[Telegram] Message from {UserId} to bot {BotId}: {Text}",
            message.From?.Id, botId, message.Text);

        try
        {
            await botClient.SendChatAction(
                message.Chat.Id,
                ChatAction.Typing,
                cancellationToken: HttpContext.RequestAborted);

            var botSession = await _session.Query<TelegramBotSession>()
                .FirstOrDefaultAsync(s => s.BotId == botId && s.TelegramUserId == message.From!.Id);

            if (botSession is null)
            {
                botSession = new TelegramBotSession
                {
                    Id = Guid.NewGuid(),
                    BotId = botId,
                    InstitutionId = bot.InstitutionId,
                    TelegramUserId = message.From!.Id,
                    TelegramUsername = message.From.Username,
                };
            }

            botSession.LastActivityAt = DateTime.UtcNow;
            botSession.Messages.Add(new TelegramSessionMessage
            {
                Role = "user",
                Content = message.Text,
                CreatedAt = DateTime.UtcNow,
            });

            _session.Store(botSession);
            await _session.SaveChangesAsync();

            var fullResponse = new StringBuilder();
            var syntheticUserId = ToGuid(message.From!.Id);

            await _agent.AskStreamingAsync(
                new TalapkerChatRequest(
                    Message: message.Text,
                    UserId: syntheticUserId,
                    InstitutionId: bot.InstitutionId
                ),
                chunk =>
                {
                    if (!string.IsNullOrEmpty(chunk.Content))
                        fullResponse.Append(chunk.Content);
                    return Task.CompletedTask;
                },
                true,
                HttpContext.RequestAborted);

            var responseText = fullResponse.ToString();

            botSession.Messages.Add(new TelegramSessionMessage
            {
                Role = "assistant",
                Content = responseText,
                CreatedAt = DateTime.UtcNow,
            });

            _session.Store(botSession);
            await _session.SaveChangesAsync();

            foreach (var chunk in SplitMessage(responseText))
            {
                await botClient.SendMessage(
                    message.Chat.Id,
                    chunk,
                    parseMode: ParseMode.Markdown,
                    cancellationToken: HttpContext.RequestAborted);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Telegram] Error processing message for bot {BotId}", botId);
            try
            {
                await botClient.SendMessage(
                    message.Chat.Id,
                    "Произошла ошибка при обработке запроса. Попробуйте позже.",
                    cancellationToken: HttpContext.RequestAborted);
            }
            catch (Exception sendEx)
            {
                _logger.LogError(sendEx, "[Telegram] Failed to send error message to user");
            }
        }

        return Ok();
    }

    private static IEnumerable<string> SplitMessage(string text, int maxLength = 4096)
    {
        for (var i = 0; i < text.Length; i += maxLength)
            yield return text.Substring(i, Math.Min(maxLength, text.Length - i));
    }
    
    private static Guid ToGuid(long telegramUserId)
    {
        var bytes = new byte[16];
        BitConverter.GetBytes(telegramUserId).CopyTo(bytes, 0);
        return new Guid(bytes);
    }
}