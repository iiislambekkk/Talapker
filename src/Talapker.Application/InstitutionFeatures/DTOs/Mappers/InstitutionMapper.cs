using Talapker.Infrastructure.Data.Institution.InstitutionEntity;

namespace Talapker.Application.InstitutionFeatures.DTOs.Mappers;

public static class InstitutionMapper
{
    public static InstitutionDto ToDto(this Institution entity, string lang)
    {
        return new InstitutionDto
        {
            Id = entity.Id,
            NationalCode = entity.NationalCode,
            Name = entity.Name.Resolve(lang),
            Description = entity.Description.Resolve(lang),
            Type = entity.Type,
            StudentsCount = entity.StudentsCount,
            MinCostPerYear = entity.MinCostPerYear,
            HasHousing = entity.HasHousing,
            HasMilitaryDepartment = entity.HasMilitaryDepartment,
            LogoKey = entity.LogoKey,
            WebSiteUrl = entity.WebSiteUrl,
            Coordinates = entity.Coordinates,
            Address = entity.Address,

            Advantages = entity.Advantages
                .Select(a => a.ToDto(lang))
                .ToList(),

            CityId = entity.CityId,
            City = entity.City?.ToShortDto(lang)
        };
    }
    
    public static InstitutionShortDto ToShortDto(this Institution entity, string lang)
    {
        return new InstitutionShortDto
        {
            Id = entity.Id,
            NationalCode = entity.NationalCode,
            Name = entity.Name.Resolve(lang),
            Description = entity.Description.Resolve(lang),
            City = entity.City?.ToShortDto(lang),
            Type = entity.Type,
            LogoKey = entity.LogoKey,
        };
    }
    
    public static InstitutionAdminDto ToAdminDto(this Institution entity, string lang)
    {
        return new InstitutionAdminDto
        {
            Id = entity.Id,
            NationalCode = entity.NationalCode,
            Name = entity.Name,
            Description = entity.Description,
            Type = entity.Type,
            StudentsCount = entity.StudentsCount,
            MinCostPerYear = entity.MinCostPerYear,
            HasHousing = entity.HasHousing,
            HasMilitaryDepartment = entity.HasMilitaryDepartment,
            LogoKey = entity.LogoKey,
            WebSiteUrl = entity.WebSiteUrl,
            Coordinates = entity.Coordinates,
            Address = entity.Address,

            Advantages = entity.Advantages
                .Select(a => a.ToAdminDto())
                .ToList(),

            CityId = entity.CityId,
            City = entity.City?.ToShortDto(lang)
        };
    }

    public static InstitutionAdvantageDto ToDto(this InstitutionAdvantage adv, string lang)
    {
        return new InstitutionAdvantageDto
        {
            Id = adv.Id,
            Title = adv.Title.Resolve(lang),
            Description = adv.Description.Resolve(lang)
        };
    }
    
    public static InstitutionAdvantageAdminDto ToAdminDto(this InstitutionAdvantage adv)
    {
        return new InstitutionAdvantageAdminDto
        {
            Id = adv.Id,
            Title = adv.Title,
            Description = adv.Description
        };
    }
}
