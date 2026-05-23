using System.Net.Http.Json;
using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Talapker.Application;
using Talapker.Application.InstitutionFeatures.DTOs;
using Talapker.Application.InstitutionFeatures.Queries.GetById;
using Talapker.Infrastructure.Vault;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Wolverine;
using ApiResponse = Talapker.Application.ApiResponse;

namespace Talapker.TelegramBot.Features;

public record AddTelegramBotCommand(Guid InstitutionId, string BotToken);

public class AddTelegramBotHandler
{
    public async Task<ApiResponse> Handle
    (
        AddTelegramBotCommand command,
        IDocumentSession session,
        TelegramClientFactory clientFactory,
        IConfiguration configuration,
        IVaultStore vaultStore,
        CancellationToken cancellationToken,
        ILogger<AddTelegramBotHandler> logger,
        IMessageBus bus
    )
    {
        ITelegramBotClient botClient;
        try
        {
            botClient = clientFactory.GetClient(command.BotToken);
            var me = await botClient.GetMe(cancellationToken);

            var existing = await session.Query<Models.TelegramBot>()
                .AnyAsync(b => b.BotToken == command.BotToken, cancellationToken);

            if (existing)
                return ApiResponse.Fail("Bot already registered.", ErrorCodes.Default);

            var isInstitutionExists = (await bus.InvokeAsync<InstitutionDto?>(
                new GetInstitutionByIdQuery(command.InstitutionId), cancellationToken)) is not null;

            if (!isInstitutionExists)
                return ApiResponse.Fail("Institution doesn't exist.", ErrorCodes.Default);

            var bot = new Models.TelegramBot
            {
                Id = Guid.NewGuid(),
                InstitutionId = command.InstitutionId,
                BotToken = "",   // не храним в БД
                BotUsername = me.Username ?? "",
                IsActive = true,
            };

            session.Store(bot);
            await session.SaveChangesAsync(cancellationToken);

            await vaultStore.StoreTokenAsync(bot.Id, command.BotToken, cancellationToken);

            
            var webhookUrl = $"{configuration["Telegram:AppUrl"]}/api/telegram/webhook/{bot.Id}";
            await botClient.SetWebhook(webhookUrl, cancellationToken: cancellationToken);

            logger.LogInformation("[Telegram] Bot @{Username} registered for institution {InstitutionId}",
                me.Username, command.InstitutionId);

            return ApiResponse.Success();
        }
        catch (ApiRequestException ex)
        {
            logger.LogError(ex, "[Telegram] Invalid bot token");
            return ApiResponse.Fail("Invalid bot token.", ErrorCodes.Default);
        }
    }
}