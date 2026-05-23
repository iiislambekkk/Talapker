using System.Text;
using System.Text.Json;
using Marten;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Talapker.Application;
using Talapker.Application.AI.Talapker;
using Talapker.Infrastructure.Data;
using Talapker.Infrastructure.Data.Assistant;
using Talapker.Infrastructure.Vault;
using Talapker.TelegramBot.Features;
using Talapker.TelegramBot.Localization;
using Talapker.TelegramBot.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
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
    private readonly TalapkerDbContext _dbContext;

    public TelegramWebhookController(
        IDocumentSession session,
        ITalapkerAgent agent,
        TelegramClientFactory clientFactory,
        ILogger<TelegramWebhookController> logger,
        IVaultStore vaultStore,
        IMessageBus messageBus,
        TalapkerDbContext dbContext)
    {
        _session = session;
        _agent = agent;
        _clientFactory = clientFactory;
        _logger = logger;
        _messageBus = messageBus;
        _vaultStore = vaultStore;
        _dbContext = dbContext;
    }

    // ── REST endpoints ────────────────────────────────────────────────────────

    [HttpGet("institutions/{institutionId:guid}")]
    public async Task<IActionResult> GetBot(Guid institutionId)
    {
        var result = await _messageBus.InvokeAsync<GetTelegramBotDto?>(new GetTelegramBotQuery(institutionId));
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> AddBot([FromBody] AddTelegramBotCommand command)
    {
        var result = await _messageBus.InvokeAsync<ApiResponse>(command);
        return result.IsSuccess ? Ok() : BadRequest(result);
    }

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

    // ── Webhook ───────────────────────────────────────────────────────────────

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

        var token = await _vaultStore.ReadTokenAsync(botId, HttpContext.RequestAborted);
        var botClient = _clientFactory.GetClient(token ?? "");

        if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery is not null)
        {
            await HandleCallbackQueryAsync(botClient, botId, bot, update.CallbackQuery);
            return Ok();
        }

        if (update.Type == UpdateType.Message && update.Message?.Text is not null)
        {
            await HandleMessageAsync(botClient, botId, bot, update.Message);
            return Ok();
        }

        return Ok();
    }

    // ── Message handler ───────────────────────────────────────────────────────

    private async Task HandleMessageAsync(
        ITelegramBotClient botClient,
        Guid botId,
        Models.TelegramBot bot,
        Message message)
    {
        var text = message.Text!.Trim();
        var chatId = message.Chat.Id;
        var ct = HttpContext.RequestAborted;

        var userSession = await GetOrCreateUserSessionAsync(botId, bot.InstitutionId, message.From!, ct);

        // /start — всегда показываем приветствие с выбором языка (если язык не выбран) или меню
        if (text.StartsWith("/start"))
        {
            if (userSession.Language is null)
                await SendLanguageSelectionAsync(botClient, chatId, ct);
            else
                await SendWelcomeAsync(botClient, chatId, userSession, ct);
            return;
        }

        // Обработка выбора языка кнопками
        if (text is "🇰🇿 Қазақша" or "🇷🇺 Русский" or "🇬🇧 English")
        {
            await HandleLanguageSelectionAsync(botClient, chatId, userSession, text, ct);
            return;
        }

        // Если язык ещё не выбран — просим выбрать
        if (userSession.Language is null)
        {
            await SendLanguageSelectionAsync(botClient, chatId, ct);
            return;
        }

        var loc = Localizations.Get(userSession.Language.Value);

        // Команды через меню (BotCommand popup)
        switch (text)
        {
            case "/programs":
                await AskAgentAndReplyAsync(botClient, botId, bot, message, userSession, loc.ProgramsPrompt, ct);
                return;
            case "/grants":
                await botClient.SendMessage(chatId, loc.GrantsPrompt, parseMode: ParseMode.Markdown, cancellationToken: ct);
                return;
            case "/documents":
                await AskAgentAndReplyAsync(botClient, botId, bot, message, userSession, loc.DocumentsPrompt, ct);
                return;
            case "/dormitory":
                await AskAgentAndReplyAsync(botClient, botId, bot, message, userSession, loc.DormitoryPrompt, ct);
                return;
            case "/deadlines":
                await AskAgentAndReplyAsync(botClient, botId, bot, message, userSession, loc.DeadlinesPrompt, ct);
                return;
            case "/contacts":
                await AskAgentAndReplyAsync(botClient, botId, bot, message, userSession, loc.ContactsPrompt, ct);
                return;
            case "/tuition":
                await AskAgentAndReplyAsync(botClient, botId, bot, message, userSession, loc.TuitionPrompt, ct);
                return;
            case "/help":
                await SendHelpAsync(botClient, chatId, userSession, ct);
                return;
            case "/language":
                await SendLanguageSelectionAsync(botClient, chatId, ct);
                return;
            case "/reset":
                await HandleResetAsync(botClient, chatId, userSession, ct);
                return;
        }

        // Свободный вопрос — идёт прямо к агенту
        await AskAgentAndReplyAsync(botClient, botId, bot, message, userSession, text, ct);
    }

    // ── Callback handler ──────────────────────────────────────────────────────

    private async Task HandleCallbackQueryAsync(
        ITelegramBotClient botClient,
        Guid botId,
        Models.TelegramBot bot,
        CallbackQuery query)
    {
        var ct = HttpContext.RequestAborted;
        var data = query.Data ?? "";
        var chatId = query.Message!.Chat.Id;

        await botClient.AnswerCallbackQuery(query.Id, cancellationToken: ct);

        var userSession = await GetOrCreateUserSessionAsync(botId, bot.InstitutionId, query.From, ct);

        // Выбор языка
        if (data.StartsWith("lang_"))
        {
            var lang = data switch
            {
                "lang_kk" => UserLanguage.Kazakh,
                "lang_ru" => UserLanguage.Russian,
                "lang_en" => UserLanguage.English,
                _ => UserLanguage.Kazakh
            };
            await SetUserLanguageAsync(botClient, chatId, query.Message.MessageId, userSession, lang, ct);
            return;
        }
    }

    // ── Welcome & help ────────────────────────────────────────────────────────

    private async Task SendWelcomeAsync(
        ITelegramBotClient botClient,
        long chatId,
        TelegramUserSession userSession,
        CancellationToken ct)
    {
        var loc = Localizations.Get(userSession.Language!.Value);
        // Просто красивое приветствие — без кнопок, без хаоса.
        // Подсказка про меню (слэш) в тексте достаточна.
        await botClient.SendMessage(
            chatId,
            loc.WelcomeMessage,
            parseMode: ParseMode.Markdown,
            cancellationToken: ct);
    }

    private async Task SendHelpAsync(
        ITelegramBotClient botClient,
        long chatId,
        TelegramUserSession userSession,
        CancellationToken ct)
    {
        var loc = Localizations.Get(userSession.Language ?? UserLanguage.Kazakh);
        await botClient.SendMessage(chatId, loc.HelpMessage, parseMode: ParseMode.Markdown, cancellationToken: ct);
    }

    private async Task HandleResetAsync(
        ITelegramBotClient botClient,
        long chatId,
        TelegramUserSession userSession,
        CancellationToken ct)
    {
        // Очищаем историю диалога — ИИ "забывает" всё
        userSession.Messages.Clear();
        userSession.LastActivityAt = DateTime.UtcNow;
        _session.Store(userSession);
        await _session.SaveChangesAsync(ct);

        var loc = Localizations.Get(userSession.Language ?? UserLanguage.Kazakh);
        await botClient.SendMessage(chatId, loc.ResetMessage, parseMode: ParseMode.Markdown, cancellationToken: ct);
        
    }

    // ── Language ──────────────────────────────────────────────────────────────

    private static async Task SendLanguageSelectionAsync(
        ITelegramBotClient botClient,
        long chatId,
        CancellationToken ct)
    {
        // Inline-кнопки для выбора языка — это правильный кейс для inline
        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("🇰🇿 Қазақша", "lang_kk"),
                InlineKeyboardButton.WithCallbackData("🇷🇺 Русский", "lang_ru"),
                InlineKeyboardButton.WithCallbackData("🇬🇧 English", "lang_en"),
            }
        });

        await botClient.SendMessage(
            chatId,
            "🌐 *Тілді таңдаңыз / Выберите язык / Select language:*",
            parseMode: ParseMode.Markdown,
            replyMarkup: keyboard,
            cancellationToken: ct);
    }

    private async Task HandleLanguageSelectionAsync(
        ITelegramBotClient botClient,
        long chatId,
        TelegramUserSession userSession,
        string text,
        CancellationToken ct)
    {
        var lang = text switch
        {
            "🇰🇿 Қазақша" => UserLanguage.Kazakh,
            "🇷🇺 Русский" => UserLanguage.Russian,
            "🇬🇧 English" => UserLanguage.English,
            _ => UserLanguage.Kazakh
        };
        await SetUserLanguageAsync(botClient, chatId, null, userSession, lang, ct);
    }

    private async Task SetUserLanguageAsync(
        ITelegramBotClient botClient,
        long chatId,
        int? messageIdToDelete,
        TelegramUserSession userSession,
        UserLanguage language,
        CancellationToken ct)
    {
        userSession.Language = language;
        _session.Store(userSession);
        await _session.SaveChangesAsync(ct);

        // Обновляем команды бота (они появятся в popup-меню)
        await SetBotCommandsAsync(botClient, language, ct);
        
        await botClient.SetChatMenuButton(
            chatId: chatId,
            menuButton: new MenuButtonWebApp
            {
                Text = "🌐 Сайт",
                WebApp = new WebAppInfo
                {
                    Url = $"https://frontend.mektep32.org/{Localizations.GetLangCode(language)}/institution/{userSession.InstitutionId}"
                }
            },
            cancellationToken: ct);

        if (messageIdToDelete.HasValue)
        {
            try { await botClient.DeleteMessage(chatId, messageIdToDelete.Value, ct); }
            catch { /* ignore */ }
        }

        var loc = Localizations.Get(language);
        // Только приветствие — никаких кнопок. Пользователь видит подсказку про / меню.
        await botClient.SendMessage(
            chatId,
            loc.WelcomeMessage,
            parseMode: ParseMode.Markdown,
            cancellationToken: ct);
    }

    private async Task SetBotCommandsAsync(ITelegramBotClient botClient, UserLanguage language, CancellationToken ct)
    {
        var loc = Localizations.Get(language);

        // Это то самое popup-меню по нажатию / в Telegram
        var commands = new[]
        {
            new BotCommand { Command = "programs",  Description = loc.CommandPrograms  },
            new BotCommand { Command = "grants",    Description = loc.CommandGrants    },
            new BotCommand { Command = "documents", Description = loc.CommandDocuments },
            new BotCommand { Command = "dormitory", Description = loc.CommandDormitory },
            new BotCommand { Command = "deadlines", Description = loc.CommandDeadlines },
            new BotCommand { Command = "contacts",  Description = loc.CommandContacts  },
            new BotCommand { Command = "tuition",   Description = loc.CommandTuition   },
            new BotCommand { Command = "language",  Description = loc.CommandLanguage  },
            new BotCommand { Command = "help",      Description = loc.CommandHelp      },
        };

        await botClient.SetMyCommands(commands, cancellationToken: ct);
    }

    // ── Agent interaction ─────────────────────────────────────────────────────

    private async Task AskAgentAndReplyAsync(
        ITelegramBotClient botClient,
        Guid botId,
        Models.TelegramBot bot,
        Message message,
        TelegramUserSession userSession,
        string userText,
        CancellationToken ct)
    {
        var chatId = message.Chat.Id;
        var loc = Localizations.Get(userSession.Language ?? UserLanguage.Kazakh);
        var syntheticUserId = ToGuid(message.From!.Id);

        try
        {
            // Показываем "печатает..."
            await botClient.SendChatAction(chatId, ChatAction.Typing, cancellationToken: ct);

            userSession.LastActivityAt = DateTime.UtcNow;
            userSession.Messages.Add(new TelegramSessionMessage
            {
                Role = "user",
                Content = userText,
                CreatedAt = DateTime.UtcNow,
            });
            _session.Store(userSession);
            await _session.SaveChangesAsync(ct);

            await SaveAssistantMessageAsync(syntheticUserId, bot.InstitutionId, "user", userText, ct);

            var agentPrompt = BuildAgentPrompt(userText, userSession.Language ?? UserLanguage.Kazakh);
            var fullResponse = new StringBuilder();

            await _agent.AskStreamingAsync(
                new TalapkerChatRequest(
                    Message: agentPrompt,
                    UserId: syntheticUserId,
                    InstitutionId: bot.InstitutionId),
                chunk =>
                {
                    if (!string.IsNullOrEmpty(chunk.Content))
                        fullResponse.Append(chunk.Content);
                    return Task.CompletedTask;
                },
                true,
                ct);

            var responseText = fullResponse.ToString();

            userSession.Messages.Add(new TelegramSessionMessage
            {
                Role = "assistant",
                Content = responseText,
                CreatedAt = DateTime.UtcNow,
            });
            _session.Store(userSession);
            await _session.SaveChangesAsync(ct);

            await SaveAssistantMessageAsync(syntheticUserId, bot.InstitutionId, "assistant", responseText, ct);

            // Отправляем ответ чанками (без кнопок после каждого сообщения)
            var chunks = SplitMessage(responseText).ToList();
            foreach (var chunk in chunks)
            {
                await SendSafeAsync(botClient, chatId, chunk, ct: ct);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Telegram] Error processing message for bot {BotId}", botId);
            try { await botClient.SendMessage(chatId, loc.ErrorMessage, cancellationToken: ct); }
            catch (Exception sendEx) { _logger.LogError(sendEx, "[Telegram] Failed to send error message"); }
        }
    }

    private async Task SaveAssistantMessageAsync(
        Guid userId,
        Guid institutionId,
        string role,
        string content,
        CancellationToken ct)
    {
        try
        {
            var msg = new AssistantChatMessage
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                InstitutionId = institutionId,
                Role = role,
                Content = content,
                CreatedAt = DateTime.UtcNow,
                ToolCalls = null
            };
            await _dbContext.AssistantChatMessage.AddAsync(msg, ct);
            await _dbContext.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Telegram] Failed to save message for user {UserId}", userId);
        }
    }

    private static string BuildAgentPrompt(string userText, UserLanguage language)
    {
        var lang = language switch
        {
            UserLanguage.Kazakh   => "Жауапты қазақ тілінде беріңіз.",
            UserLanguage.Russian  => "Отвечай на русском языке.",
            UserLanguage.English  => "Respond in English.",
            _ => "Жауапты қазақ тілінде беріңіз."
        };
        return $"{lang}\n\nUser question: {userText}";
    }

    // ── Session ───────────────────────────────────────────────────────────────

    private async Task<TelegramUserSession> GetOrCreateUserSessionAsync(
        Guid botId,
        Guid institutionId,
        User from,
        CancellationToken ct)
    {
        var session = await _session.Query<TelegramUserSession>()
            .FirstOrDefaultAsync(s => s.BotId == botId && s.TelegramUserId == from.Id, ct);

        if (session is not null)
            return session;

        session = new TelegramUserSession
        {
            Id = Guid.NewGuid(),
            BotId = botId,
            InstitutionId = institutionId,
            TelegramUserId = from.Id,
            TelegramUsername = from.Username,
            FirstName = from.FirstName,
            LastName = from.LastName,
            Language = null,
            CreatedAt = DateTime.UtcNow,
            LastActivityAt = DateTime.UtcNow,
            Messages = new List<TelegramSessionMessage>()
        };

        _session.Store(session);
        await _session.SaveChangesAsync(ct);
        return session;
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

    private async Task SendSafeAsync(
        ITelegramBotClient bot,
        long chatId,
        string text,
        InlineKeyboardMarkup? replyMarkup = null,
        CancellationToken ct = default)
    {
        try
        {
            await bot.SendMessage(chatId, text,
                parseMode: ParseMode.Markdown,
                replyMarkup: replyMarkup,
                cancellationToken: ct);
        }
        catch (Exception ex) when (ex.Message.Contains("can't parse entities"))
        {
            _logger.LogWarning("[Telegram] Markdown parse failed, retrying as plain text");
            await bot.SendMessage(chatId, StripMarkdown(text),
                parseMode: ParseMode.None,
                replyMarkup: replyMarkup,
                cancellationToken: ct);
        }
    }

    private static string StripMarkdown(string text) =>
        System.Text.RegularExpressions.Regex.Replace(text, @"[*_`~\[\]()]", "");
}

