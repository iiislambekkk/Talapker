using Microsoft.EntityFrameworkCore;
using Talapker.Application.InstitutionFeatures.DTOs;
using Talapker.Application.InstitutionFeatures.DTOs.Mappers;
using Talapker.Infrastructure.Auth;
using Talapker.Infrastructure.Data;

namespace Talapker.Application.ProspectFeatures.Queries;

public record GetAllSubscribedInstitutionsQuery(Guid UserId);

public class GetAllSubscribedInstitutionsHandler
{
    public async Task<List<InstitutionDto>> Handle(
        GetAllSubscribedInstitutionsQuery query,
        TalapkerDbContext db,  ILanguageContext languageContext,
        CancellationToken ct = default)
    {
        var institutions = await db.Users
            .Where(u => u.Id == query.UserId.ToString())
            .SelectMany(u => u.SubscribedInstitutions)
            .Include(i => i.Advantages)
            .Include(i => i.City)
            .ToListAsync(ct);

        return institutions
            .Select(i => i.ToDto(languageContext.Language))
            .ToList();
    }
}