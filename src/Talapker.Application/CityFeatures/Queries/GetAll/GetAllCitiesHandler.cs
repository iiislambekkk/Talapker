using Microsoft.EntityFrameworkCore;
using Talapker.Application.InstitutionFeatures.DTOs;
using Talapker.Application.InstitutionFeatures.DTOs.Mappers;
using Talapker.Infrastructure.Auth;
using Talapker.Infrastructure.Data;
using Talapker.Infrastructure.Data.Institution;

namespace Talapker.Application.CityFeatures.Queries.GetAll;

public class GetAllCitiesHandler
{
    public async Task<List<City>> Handle(GetAllCitiesQuery query, TalapkerDbContext db)
    {
        var cities = await db.Cities.ToListAsync();
        return cities;
    }
}