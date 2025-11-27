using Microsoft.EntityFrameworkCore;
using Talapker.Application.InstitutionFeatures.DTOs;
using Talapker.Application.InstitutionFeatures.DTOs.Mappers;
using Talapker.Application.InstitutionFeatures.Queries.GetById;
using Talapker.Infrastructure.Auth;
using Talapker.Infrastructure.Data;

namespace Talapker.Application.InstitutionFeatures.Queries.GetByIdForAdmin;

public class GetByIdForAdminQueryHandler
{
    public async Task<InstitutionAdminDto?> Handle(GetInstitutionByIdForAdminQuery query, TalapkerDbContext db, ILanguageContext languageContext)
    {
        var institution = await db.Institutions.Include(i => i.City).AsNoTracking().FirstOrDefaultAsync(u => u.Id == query.Id);

        return institution?.ToAdminDto(languageContext.Language);
    }
}