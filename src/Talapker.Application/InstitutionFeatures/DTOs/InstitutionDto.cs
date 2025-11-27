using Talapker.Infrastructure.Data.Institution.InstitutionEntity;

namespace Talapker.Application.InstitutionFeatures.DTOs;

public class InstitutionDto
{
    public Guid Id { get; set; }

    public int? NationalCode { get; set; }
    public string Name { get; set; } = String.Empty;
    public string Description { get; set; }= String.Empty;
    public InstitutionType Type { get; set; }

    public int StudentsCount { get; set; }
    public long MinCostPerYear { get; set; }

    public bool? HasHousing { get; set; }
    public bool? HasMilitaryDepartment { get; set; }

    public string LogoKey { get; set; } = string.Empty;
    public string WebSiteUrl { get; set; } = string.Empty;
    public string Coordinates { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;

    public List<InstitutionAdvantageDto> Advantages { get; set; } = new();

    public Guid? CityId { get; set; }
    public CityShortDto? City { get; set; }
}
