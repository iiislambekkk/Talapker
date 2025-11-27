using Microsoft.EntityFrameworkCore;
using Talapker.Application.InstitutionFeatures.DTOs;
using Talapker.Application.InstitutionFeatures.DTOs.Mappers;
using Talapker.Infrastructure.Auth;
using Talapker.Infrastructure.Data;

namespace Talapker.Application.InstitutionFeatures.Queries.GetAllInstitutions;

public class GetAllInstitutionsHandler
{
    public async Task<List<InstitutionShortDto>> Handle(GetAllInstitutionsQuery query, TalapkerDbContext db, ILanguageContext languageContext)
    {
        return await db.Institutions.AsNoTracking().Select(i => i.ToShortDto(languageContext.Language)).ToListAsync();
    }
}