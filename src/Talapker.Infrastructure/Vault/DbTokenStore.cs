using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Talapker.Infrastructure.Data;
using Talapker.Infrastructure.Data.Tg;

namespace Talapker.Infrastructure.Vault;

public class DbTokenStore : IVaultStore
{
    private readonly TalapkerDbContext _db;
    private readonly ILogger<DbTokenStore> _logger;
    private readonly byte[] _encryptionKey;

    public DbTokenStore(TalapkerDbContext db, ILogger<DbTokenStore> logger, IConfiguration config)
    {
        _db = db;
        _logger = logger;
        var key = config["TokenEncryption:Key"]!;
        _encryptionKey = SHA256.HashData(Encoding.UTF8.GetBytes(key)); // всегда 32 байта
    }

    public async Task StoreTokenAsync(Guid botId, string token, CancellationToken ct = default)
    {
        _logger.LogInformation("[DbTokenStore] Storing token for bot {BotId}", botId);
        try
        {
            var encrypted = Encrypt(token);
            var existing = await _db.BotTokens.FindAsync([botId], ct);

            if (existing is null)
            {
                _db.BotTokens.Add(new BotToken
                {
                    BotId = botId,
                    EncryptedToken = encrypted,
                    CreatedAt = DateTime.UtcNow
                });
            }
            else
            {
                existing.EncryptedToken = encrypted;
                existing.UpdatedAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync(ct);
            _logger.LogInformation("[DbTokenStore] Token stored successfully for bot {BotId}", botId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[DbTokenStore] Failed to store token for bot {BotId}", botId);
            throw;
        }
    }

    public async Task<string?> ReadTokenAsync(Guid botId, CancellationToken ct = default)
    {
        _logger.LogInformation("[DbTokenStore] Reading token for bot {BotId}", botId);
        try
        {
            var record = await _db.BotTokens
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.BotId == botId, ct);

            if (record is null)
            {
                _logger.LogWarning("[DbTokenStore] Token not found for bot {BotId}", botId);
                return null;
            }

            _logger.LogInformation("[DbTokenStore] Token found for bot {BotId}", botId);
            return Decrypt(record.EncryptedToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[DbTokenStore] Unexpected error reading token for bot {BotId}", botId);
            return null;
        }
    }

    public async Task DeleteTokenAsync(Guid botId, CancellationToken ct = default)
    {
        _logger.LogInformation("[DbTokenStore] Deleting token for bot {BotId}", botId);
        try
        {
            var record = await _db.BotTokens.FindAsync([botId], ct);
            if (record is null)
            {
                _logger.LogWarning("[DbTokenStore] Token not found on delete for bot {BotId}", botId);
                return;
            }

            _db.BotTokens.Remove(record);
            await _db.SaveChangesAsync(ct);
            _logger.LogInformation("[DbTokenStore] Token deleted for bot {BotId}", botId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[DbTokenStore] Unexpected error deleting token for bot {BotId}", botId);
        }
    }

    private string Encrypt(string plainText)
    {
        var iv = RandomNumberGenerator.GetBytes(16);
        using var aes = Aes.Create();
        aes.Key = _encryptionKey;
        aes.IV = iv;

        var encrypted = aes.EncryptCbc(Encoding.UTF8.GetBytes(plainText), iv);
        return Convert.ToBase64String(iv) + ":" + Convert.ToBase64String(encrypted);
    }

    private string Decrypt(string cipherText)
    {
        var parts = cipherText.Split(':');
        var iv = Convert.FromBase64String(parts[0]);
        var encrypted = Convert.FromBase64String(parts[1]);

        using var aes = Aes.Create();
        aes.Key = _encryptionKey;
        aes.IV = iv;

        return Encoding.UTF8.GetString(aes.DecryptCbc(encrypted, iv));
    }
}