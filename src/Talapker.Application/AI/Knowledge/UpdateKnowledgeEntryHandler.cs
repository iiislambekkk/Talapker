using Microsoft.EntityFrameworkCore;
using Talapker.Infrastructure.Data;
using Wolverine;

namespace Talapker.Application.AI.Knowledge;


public class UpdateKnowledgeEntryHandler
{
    public async Task<UpdateKnowledgeEntryResult> Handle
    (
        UpdateKnowledgeEntry message, 
        TalapkerDbContext db,
        IMessageBus bus
    )
    {
        var entry = await db.KnowledgeEntries
            .Include(e => e.SourceFile)
            .FirstOrDefaultAsync(e => e.Id == message.EntryId && e.InstitutionId == message.InstitutionId);

        if (entry is null)
            throw new InvalidOperationException("Запись не найдена");

        entry.Question = message.Question;
        entry.Answer = message.Answer;
        entry.Tags = message.Tags.ToArray();
        entry.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        await bus.PublishAsync(new ReEmbedKnowledgeEntry(entry.Id));

        return new UpdateKnowledgeEntryResult(
            entry.Id,
            entry.InstitutionId,
            entry.SourceFileId,
            entry.SourceFile?.FileName,
            entry.SourceFile?.StorageKey,
            entry.Question,
            entry.Answer,
            entry.Tags,
            entry.CreatedAt,
            entry.UpdatedAt
        );
    }
}