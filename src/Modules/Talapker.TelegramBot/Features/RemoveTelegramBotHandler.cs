using Marten;
using Microsoft.Extensions.Logging;
using Talapker.Application;
using Talapker.Infrastructure.Vault;
using Talapker.TelegramBot.Models;
using Telegram.Bot;

namespace Talapker.TelegramBot.Features;

public record RemoveTelegramBotCommand(Guid BotId, Guid InstitutionId);

public class RemoveTelegramBotHandler
{
    public async Task<ApiResponse> Handle(
        RemoveTelegramBotCommand command,
        IDocumentSession session,
        TelegramClientFactory clientFactory,
        IVaultStore vaultStore,
        CancellationToken cancellationToken,
        ILogger<RemoveTelegramBotHandler> logger)
    {
        var bot = await session.Query<Models.TelegramBot>()
            .FirstOrDefaultAsync(b => b.Id == command.BotId && b.InstitutionId == command.InstitutionId,
                cancellationToken);

        if (bot is null)
        {
            logger.LogWarning("[RemoveBot] Bot not found. BotId={BotId}", command.BotId);
            return ApiResponse.Fail("Bot not found.", ErrorCodes.Default);
        }

        var token = await vaultStore.ReadTokenAsync(bot.Id, cancellationToken);

        if (token is not null)
        {
            try
            {
                var botClient = clientFactory.GetClient(token);
                await botClient.DeleteWebhook(cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[RemoveBot] Failed to delete webhook for bot {BotId}", bot.Id);
            }

            await vaultStore.DeleteTokenAsync(bot.Id, cancellationToken);
        }
        
        var sessionIds = await session.Query<TelegramBotSession>()
            .Where(s => s.BotId == command.BotId)
            .Select(s => s.Id)
            .ToListAsync(cancellationToken);

        foreach (var id in sessionIds)
            session.Delete<TelegramBotSession>(id);

        session.Delete<Models.TelegramBot>(bot.Id);
        await session.SaveChangesAsync(cancellationToken);

        logger.LogInformation("[RemoveBot] Bot @{Username} removed", bot.BotUsername);
        return ApiResponse.Success();
    }
}