using Talapker.Infrastructure.Data;
using Talapker.Infrastructure.Data.Assistant;
using Wolverine;

namespace Talapker.Application.AI.Knowledge;


public class CreateKnowledgeEntryHandler
{
    public async Task<CreateKnowledgeEntryResult> Handle(CreateKnowledgeEntry message, TalapkerDbContext db, IMessageBus bus)
    {
        var institution = await db.Institutions.FindAsync(message.InstitutionId);
        if (institution is null)
            throw new InvalidOperationException("Учреждение не найдено");

        var entry = new KnowledgeEntry
        {
            InstitutionId = message.InstitutionId,
            Question = message.Question,
            Answer = message.Answer,
            Tags = message.Tags.ToArray()
        };

        db.KnowledgeEntries.Add(entry);
        await db.SaveChangesAsync();

        await bus.PublishAsync(new ReEmbedKnowledgeEntry(entry.Id));

        return new CreateKnowledgeEntryResult(
            entry.Id,
            entry.InstitutionId,
            entry.Question,
            entry.Answer,
            entry.Tags,
            entry.CreatedAt
        );
    }
}