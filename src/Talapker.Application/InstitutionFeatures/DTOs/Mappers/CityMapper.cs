using Talapker.Infrastructure.Data.Institution;

namespace Talapker.Application.InstitutionFeatures.DTOs.Mappers;

public static class CityMapper
{
    public static CityShortDto ToShortDto(this City city, string lang)
    {
        return new CityShortDto
        {
            Id = city.Id,
            Name = city.Name.Resolve(lang),
            LogoKey = city.LogoKey
        };
    }
}