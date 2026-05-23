namespace Talapker.Infrastructure.Data.Assistant;


public class KnowledgeFile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid InstitutionId { get; set; }
    public Guid? UploadedById { get; set; }
    public Institution.Institution Institution { get; set; } = null!;
    public string FileName { get; set; } = string.Empty;
    public string StorageKey { get; set; } = string.Empty;
    public string TextContent { get; set; } = string.Empty;
    public KnowledgeFileStatus Status { get; set; } = KnowledgeFileStatus.Pending;
    public string? ErrorMessage { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public string ContentType { get; set; } = "application/octet-stream";
    public DateTime? ProcessedAt { get; set; }
    public ICollection<KnowledgeEntry> Entries { get; set; } = [];
}

public enum KnowledgeFileStatus
{
    Pending,
    Processing,
    Processed,
    Failed
}

public class KnowledgeEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid InstitutionId { get; set; }
    public Institution.Institution Institution { get; set; } = null!;
    public Guid? SourceFileId { get; set; }
    public KnowledgeFile? SourceFile { get; set; }
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public string[] Tags { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}