using ImTools;
using Microsoft.EntityFrameworkCore;
using Talapker.Application.InstitutionFeatures.DTOs;
using Talapker.Application.InstitutionFeatures.DTOs.Mappers;
using Talapker.Application.InstitutionFeatures.Queries.GetAllInstitutions;
using Talapker.Infrastructure.Auth;
using Talapker.Infrastructure.Data;

namespace Talapker.Application.InstitutionFeatures.Queries.GetById;

public class GetInstitutionByIdQueryHandler
{
    public async Task<InstitutionDto?> Handle(GetInstitutionByIdQuery query, TalapkerDbContext db, ILanguageContext languageContext)
    {
        var institution = await db.Institutions.Include(i => i.City).AsNoTracking().FirstOrDefaultAsync(u => u.Id == query.Id);

        return institution?.ToDto(languageContext.Language);
    }
}