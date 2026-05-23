using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Talapker.Infrastructure.Data.Institution;

namespace Talapker.Application.AI.Talapker;

public record ProgramCacheDto(Guid Id, string Code, string Name);

public record InstitutionContextCacheEntry(Institution Institution, List<ProgramCacheDto> Programs)
{   
    public (Institution, List<ProgramCacheDto>) ToDomain() => (Institution, Programs);
}

public static class TalapkerToolsCache
{
    private const string AllProgramKeysIndex = "programs:all-keys";

    private static readonly DistributedCacheEntryOptions IndexOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
    };


    public static string ProgramQueryKey(string query) =>
        $"programs:query:{query.ToLowerInvariant()}";

    public static string GrantGroupKey(Guid groupId) =>
        $"grants:group:{groupId}";


    public static async Task RegisterProgramQueryKeyAsync(IDistributedCache cache, string query, CancellationToken ct = default)
    {
        var key = ProgramQueryKey(query);
        var raw = await cache.GetStringAsync(AllProgramKeysIndex, ct);
        var keys = raw is not null
            ? JsonSerializer.Deserialize<HashSet<string>>(raw)!
            : [];

        if (keys.Add(key))
            await cache.SetStringAsync(AllProgramKeysIndex, JsonSerializer.Serialize(keys), IndexOptions, ct);
    }


    public static async Task InvalidateAllProgramsAsync(IDistributedCache cache, CancellationToken ct = default)
    {
        var raw = await cache.GetStringAsync(AllProgramKeysIndex, ct);
        if (raw is null) return;

        var keys = JsonSerializer.Deserialize<HashSet<string>>(raw)!;
        foreach (var key in keys)
            await cache.RemoveAsync(key, ct);

        await cache.RemoveAsync(AllProgramKeysIndex, ct);
    }

    public static Task InvalidateGrantGroupAsync(IDistributedCache cache, Guid groupId, CancellationToken ct = default) =>
        cache.RemoveAsync(GrantGroupKey(groupId), ct);
    
    
    private const string InstitutionContextPrefix = "institution:context:";

    private static readonly DistributedCacheEntryOptions InstitutionContextOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(6)
    };

    public static string InstitutionContextKey(Guid institutionId) =>
        $"{InstitutionContextPrefix}{institutionId}";

    public static async Task<(Institution Institution, List<ProgramCacheDto> Programs)> GetOrSetInstitutionContextAsync(
        IDistributedCache cache,
        Guid institutionId,
        Func<Task<(Institution, List<ProgramCacheDto>)>> factory,
        CancellationToken ct = default)
    {
        var key    = InstitutionContextKey(institutionId);
        var cached = await cache.GetStringAsync(key, ct);

        if (cached is not null)
            return JsonSerializer.Deserialize<InstitutionContextCacheEntry>(cached)!.ToDomain();

        var (institution, programs) = await factory();
        var entry = new InstitutionContextCacheEntry(institution, programs);
        await cache.SetStringAsync(key, JsonSerializer.Serialize(entry), InstitutionContextOptions, ct);

        return (institution, programs);
    }

    public static Task InvalidateInstitutionContextAsync(IDistributedCache cache, Guid institutionId, CancellationToken ct = default) =>
        cache.RemoveAsync(InstitutionContextKey(institutionId), ct);
}