using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VaultSharp.Core;

namespace Talapker.Infrastructure.Vault;
using VaultSharp;
using VaultSharp.V1.AuthMethods.Token;

public interface IVaultStore
{
    Task StoreTokenAsync(Guid botId, string token, CancellationToken ct = default);
    Task<string?> ReadTokenAsync(Guid botId, CancellationToken ct = default);
    Task DeleteTokenAsync(Guid botId, CancellationToken ct = default);
}

public class VaultStore : IVaultStore
{
    private readonly IVaultClient _client;
    private readonly ILogger<VaultStore> _logger;
    private const string MountPoint = "secret";

    public VaultStore(IConfiguration config, ILogger<VaultStore> logger)
    {
        _logger = logger;
        var address = config["Vault:Address"]!;
        var token   = config["Vault:Token"]!;
        _logger.LogInformation("[Vault] Initializing client with address: {Address}", address);
        _client = new VaultClient(new VaultClientSettings(address, new TokenAuthMethodInfo(token)));
    }

    public async Task StoreTokenAsync(Guid botId, string token, CancellationToken ct = default)
    {
        _logger.LogInformation("[Vault] Storing token for bot {BotId}", botId);
        try
        {
            await _client.V1.Secrets.KeyValue.V2.WriteSecretAsync(
                path: $"bots/{botId}",
                data: new Dictionary<string, object> { ["token"] = token },
                mountPoint: MountPoint
            );
            _logger.LogInformation("[Vault] Token stored successfully for bot {BotId}", botId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Vault] Failed to store token for bot {BotId}", botId);
            throw;
        }
    }

    public async Task<string?> ReadTokenAsync(Guid botId, CancellationToken ct = default)
    {
        _logger.LogInformation("[Vault] Reading token for bot {BotId}", botId);
        try
        {
            var secret = await _client.V1.Secrets.KeyValue.V2.ReadSecretAsync(
                path: $"bots/{botId}",
                mountPoint: MountPoint
            );
            _logger.LogInformation("[Vault] Token found for bot {BotId}", botId);
            return secret.Data.Data["token"].ToString();
        }
        catch (VaultApiException ex)
        {
            _logger.LogWarning("[Vault] Token not found for bot {BotId}. Status: {Status}, Errors: {Errors}",
                botId, ex.HttpStatusCode, ex.Message);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Vault] Unexpected error reading token for bot {BotId}", botId);
            return null;
        }
    }

    public async Task DeleteTokenAsync(Guid botId, CancellationToken ct = default)
    {
        _logger.LogInformation("[Vault] Deleting token for bot {BotId}", botId);
        try
        {
            await _client.V1.Secrets.KeyValue.V2.DeleteSecretAsync(
                path: $"bots/{botId}",
                mountPoint: MountPoint
            );
            _logger.LogInformation("[Vault] Token deleted for bot {BotId}", botId);
        }
        catch (VaultApiException ex)
        {
            _logger.LogWarning("[Vault] Token not found on delete for bot {BotId}. Status: {Status}",
                botId, ex.HttpStatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Vault] Unexpected error deleting token for bot {BotId}", botId);
        }
    }
}