using Talapker.Infrastructure.Data.Institution.InstitutionEntity;

namespace Talapker.Application.InstitutionFeatures.DTOs;

public class InstitutionShortDto
{
    public Guid Id { get; set; }

    public int? NationalCode { get; set; }
    public string Name { get; set; } = String.Empty;
    public string Description { get; set; } = String.Empty;
    public CityShortDto? City { get; set; } 
    public InstitutionType Type { get; set; }
    public string LogoKey { get; set; } = string.Empty;
}