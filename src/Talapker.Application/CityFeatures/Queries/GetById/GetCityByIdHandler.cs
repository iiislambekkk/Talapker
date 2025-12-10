using Microsoft.EntityFrameworkCore;
using Talapker.Infrastructure.Data;
using Talapker.Infrastructure.Data.Institution;

namespace Talapker.Application.CityFeatures.Queries.GetById;

public class GetCityByIdHandler
{
    public async Task<City?> Handle(GetCityByIdQuery query, TalapkerDbContext db, CancellationToken ct)
    {
        return  await db.Cities.Include(c => c.Institutions).FirstOrDefaultAsync(c => c.Id == query.Id);
    }
}