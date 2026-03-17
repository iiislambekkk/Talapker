using Microsoft.EntityFrameworkCore;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using Talapker.Infrastructure.Data;
using Microsoft.Extensions.Configuration;

namespace Talapker.Application.AI.Knowledge;

public class DeleteKnowledgeEntryHandler
{
    private const string QdrantCollection = "knowledge";

    public async Task Handle(DeleteKnowledgeEntry message, TalapkerDbContext db, IConfiguration configuration)
    {
        var entry = await db.KnowledgeEntries
            .FirstOrDefaultAsync(e => e.Id == message.EntryId && e.InstitutionId == message.InstitutionId);

        if (entry is null)
            throw new InvalidOperationException("Запись не найдена");

        var qdrant = new QdrantClient(
            configuration["Qdrant:Host"] ?? "localhost",
            int.Parse(configuration["Qdrant:Port"] ?? "6334")
        );
        
        await qdrant.DeleteAsync(QdrantCollection, [new PointId { Uuid = entry.Id.ToString() }]);

        db.KnowledgeEntries.Remove(entry);
        await db.SaveChangesAsync();
    }
}