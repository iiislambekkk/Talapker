using Talapker.Infrastructure.Data.Assistant;

namespace Talapker.Application.AI.Knowledge;

// ── Wolverine messages ─────────────────────────────────────────────────────

public record ProcessKnowledgeFile(Guid FileId);

public record EmbedKnowledgeEntry(
    Guid InstitutionId,
    Guid SourceFileId,
    string Question,
    string Answer,
    List<string> Tags
);

public record ReEmbedKnowledgeEntry(Guid EntryId);

public record CreateKnowledgeEntry(
    Guid InstitutionId,
    string Question,
    string Answer,
    List<string> Tags
);

public record UpdateKnowledgeEntry(
    Guid InstitutionId,
    Guid EntryId,
    string Question,
    string Answer,
    List<string> Tags
);

public record DeleteKnowledgeEntry(Guid InstitutionId, Guid EntryId);

public record DeleteKnowledgeFile(Guid InstitutionId, Guid FileId);

public record AskKnowledge(Guid InstitutionId, string Question);

// ── Handler results ────────────────────────────────────────────────────────

public record CreateKnowledgeEntryResult(
    Guid EntryId,
    Guid InstitutionId,
    string Question,
    string Answer,
    string[] Tags,
    DateTime CreatedAt
);

public record UpdateKnowledgeEntryResult(
    Guid EntryId,
    Guid InstitutionId,
    Guid? SourceFileId,
    string? SourceFileName,
    string? SourceFileKey,
    string Question,
    string Answer,
    string[] Tags,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record AskKnowledgeResult(string Answer, List<KnowledgeSourceRef> Sources);

public record KnowledgeSourceRef(string FileName, string FileKey, string MatchedQuestion);

// ── HTTP DTOs ──────────────────────────────────────────────────────────────

public record UploadKnowledgeFileResponse(
    Guid FileId,
    string FileName,
    string StorageKey,
    string Status
);

public record KnowledgeFileDto(
    Guid Id,
    string FileName,
    string StorageKey,
    string Status,
    string? ErrorMessage,
    DateTime UploadedAt,
    DateTime? ProcessedAt,
    int EntriesCount
);

public record KnowledgeEntryDto(
    Guid Id,
    Guid? SourceFileId,
    string? SourceFileName,
    string? SourceFileKey,
    string Question,
    string Answer,
    string[] Tags,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record KnowledgeEntriesQuery(
    Guid? FileId,
    string? Tag,
    bool? ManualOnly, 
    int Page = 1,
    int PageSize = 20
);

public record CreateKnowledgeEntryRequest(
    string Question,
    string Answer,
    List<string> Tags
);

public record UpdateKnowledgeEntryRequest(
    string Question,
    string Answer,
    List<string> Tags
);

public record AskKnowledgeRequest(string Question);

public record AskKnowledgeResponse(
    string Answer,
    List<KnowledgeSourceRef> Sources
);

public record PagedResult<T>(
    List<T> Items,
    int TotalCount,
    int Page,
    int PageSize
)
{
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}