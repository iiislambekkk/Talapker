using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VaultSharp;
using VaultSharp.Core;
using VaultSharp.V1.AuthMethods.Token;

namespace Talapker.Infrastructure.Secrets;

public interface ISecretProvider
{
    Task<string> GetRequiredAsync(string key, CancellationToken ct = default);
    Task<string?> GetOrDefaultAsync(string key, CancellationToken ct = default);
}

public class SecretProvider : ISecretProvider
{
    private readonly IVaultClient _client;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SecretProvider> _logger;
    private const string MountPoint = "secret";

    public SecretProvider(IConfiguration configuration, ILogger<SecretProvider> logger)
    {
        _configuration = configuration;
        _logger = logger;

        var address = configuration["Vault:Address"]!;
        var token = configuration["Vault:Token"]!;
        _client = new VaultClient(new VaultClientSettings(address, new TokenAuthMethodInfo(token)));
    }

    public async Task<string> GetRequiredAsync(string key, CancellationToken ct = default)
    {
        var value = await GetOrDefaultAsync(key, ct);
        if (value is null)
            throw new InvalidOperationException($"Secret '{key}' not found in Vault or configuration.");
        return value;
    }

    public async Task<string?> GetOrDefaultAsync(string key, CancellationToken ct = default)
    {
        // Vault path: "Firebase:KeyPath" → "Firebase/KeyPath"
        var vaultPath = key.Replace(":", "/");

        try
        {
            _logger.LogDebug("[SecretProvider] Trying Vault for key '{Key}' at path '{Path}'", key, vaultPath);

            var secret = await _client.V1.Secrets.KeyValue.V2.ReadSecretAsync(
                path: vaultPath,
                mountPoint: MountPoint
            );

            if (secret.Data.Data.TryGetValue("value", out var vaultValue) && vaultValue is not null)
            {
                _logger.LogDebug("[SecretProvider] Key '{Key}' resolved from Vault", key);
                return vaultValue.ToString();
            }
        }
        catch (VaultApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogDebug("[SecretProvider] Key '{Key}' not found in Vault, falling back to configuration", key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[SecretProvider] Vault error for key '{Key}', falling back to configuration", key);
        }

        var configValue = _configuration[key];
        if (configValue is not null)
        {
            _logger.LogDebug("[SecretProvider] Key '{Key}' resolved from configuration", key);
            return configValue;
        }

        _logger.LogWarning("[SecretProvider] Key '{Key}' not found in Vault or configuration", key);
        return null;
    }
}